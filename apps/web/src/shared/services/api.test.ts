import { server } from "@test/server";
import axios from "axios";
import { delay, HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { ApiError, api, shouldRetryQuery, UnauthorizedError } from "./api";

const url = "http://localhost:5000/api/v1/probe";

describe("api", () => {
  it("preserves problem details and field validation", async () => {
    server.use(
      http.get(url, () =>
        HttpResponse.json(
          { detail: "Corrija os campos", errors: { name: "Obrigatório" } },
          { status: 422 },
        ),
      ),
    );
    await expect(api.get("/probe")).rejects.toMatchObject({
      status: 422,
      details: { errors: { name: "Obrigatório" } },
      kind: "http",
    });
    await expect(api.get("/probe")).rejects.toBeInstanceOf(ApiError);
  });

  it("classifies unauthorized responses", async () => {
    server.use(http.get(url, () => new HttpResponse(null, { status: 401 })));
    await expect(api.get("/probe")).rejects.toBeInstanceOf(UnauthorizedError);
  });

  it("classifies network failures", async () => {
    server.use(http.get(url, () => HttpResponse.error()));
    await expect(api.get("/probe")).rejects.toMatchObject({
      kind: "network",
      status: undefined,
    });
  });

  it("forwards cancellation without turning it into a connection error", async () => {
    server.use(
      http.get(url, async () => {
        await delay("infinite");
        return HttpResponse.json({});
      }),
    );
    const controller = new AbortController();
    const request = api.get("/probe", controller.signal);
    controller.abort();
    await expect(request).rejects.toSatisfy(axios.isCancel);
  });

  it("times out reads after thirty seconds", async () => {
    vi.useFakeTimers();
    try {
      server.use(
        http.get(url, async () => {
          await delay("infinite");
          return HttpResponse.json({});
        }),
      );
      const result = expect(api.get("/probe")).rejects.toMatchObject({
        kind: "timeout",
      });
      await vi.advanceTimersByTimeAsync(30_001);
      await result;
    } finally {
      vi.useRealTimers();
    }
  });

  it("does not abort a pending write after thirty seconds", async () => {
    vi.useFakeTimers();
    let release: () => void = () => {};
    const pending = new Promise<void>((resolve) => {
      release = resolve;
    });
    try {
      server.use(
        http.post(url, async () => {
          await pending;
          return new HttpResponse(null, { status: 204 });
        }),
      );
      let settled = false;
      const request = api.post("/probe", {}).finally(() => {
        settled = true;
      });
      await vi.advanceTimersByTimeAsync(30_001);
      expect(settled).toBe(false);
      release();
      await expect(request).resolves.toBeUndefined();
    } finally {
      release();
      vi.useRealTimers();
    }
  });

  it("reports a lost write response as an unknown outcome without encouraging a retry", async () => {
    server.use(http.post(url, () => HttpResponse.error()));
    const error = await api.post("/probe", {}).catch((error: unknown) => error);
    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ kind: "unknown-outcome" });

    expect(shouldRetryQuery(0, error as Error)).toBe(false);
  });

  it("reports a lost response of an idempotent write as a connection failure", async () => {
    server.use(http.post(url, () => HttpResponse.error()));
    await expect(
      api.post("/probe", {}, { idempotent: true }),
    ).rejects.toMatchObject({ kind: "network" });
  });

  it("reports a write attempted while offline as offline rather than an unknown outcome", async () => {
    vi.spyOn(navigator, "onLine", "get").mockReturnValue(false);
    server.use(http.post(url, () => HttpResponse.error()));
    const error = await api.post("/probe", {}).catch((error: unknown) => error);
    expect(error).toMatchObject({ kind: "offline" });
  });

  it("keeps the status when the error body is not problem details", async () => {
    server.use(
      http.get(
        url,
        () =>
          new HttpResponse("<html>Bad Gateway</html>", {
            status: 502,
            headers: { "Content-Type": "text/html" },
          }),
      ),
    );
    await expect(api.get("/probe")).rejects.toMatchObject({
      status: 502,
      details: undefined,
      message: "HTTP 502",
    });
  });

  it("treats a browser-aborted XHR as cancellation rather than a timeout", async () => {
    vi.spyOn(XMLHttpRequest.prototype, "send").mockImplementation(function (
      this: XMLHttpRequest,
    ) {
      this.dispatchEvent(new ProgressEvent("abort"));
    });
    const error = await api.get("/probe").catch((error: unknown) => error);
    expect(axios.isCancel(error)).toBe(true);
    expect(shouldRetryQuery(0, error as Error)).toBe(false);
  });

  it.each([400, 401, 403, 404, 409, 422, 429])(
    "does not retry permanent HTTP %s failures",
    (status) => {
      expect(shouldRetryQuery(0, new ApiError("failure", status))).toBe(false);
    },
  );

  it.each([408, 500, 502, 503, 504])(
    "limits transient HTTP %s failures to two retries",
    (status) => {
      const error = new ApiError("failure", status);
      expect(shouldRetryQuery(0, error)).toBe(true);
      expect(shouldRetryQuery(1, error)).toBe(true);
      expect(shouldRetryQuery(2, error)).toBe(false);
    },
  );

  it("only retries known recoverable failures", () => {
    expect(
      shouldRetryQuery(
        0,
        new ApiError("offline", undefined, undefined, "network"),
      ),
    ).toBe(true);
    expect(shouldRetryQuery(0, new Error("programming error"))).toBe(false);
    expect(shouldRetryQuery(0, new axios.CanceledError())).toBe(false);
  });
});
