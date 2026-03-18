import { ApiRoutes } from "@/lib/api/routes";
import {
  getClientBasicAuthHeaderValue,
  HCLCSSFPost,
  HCLCSSFPostWithSession,
  requireAccessToken
} from "@/lib/api/client";
import { resolveRevocationEndpoint } from "@/lib/oidc";
import { env } from "@/lib/env";
import { HCLCSSFFetch } from "@/lib/server-fetch";
import { type PagingModel, type TokenModel } from "@/lib/types/HCL.CS.SF";

type ClientTokenLookupRequest = {
  clients_list: string[];
  paging_model?: PagingModel;
};

type UserTokenLookupRequest = {
  user_list: string[];
  paging_model?: PagingModel;
};

type DateRangeLookupRequest = {
  from_date: string;
  to_date: string;
  paging_model?: PagingModel;
};

export async function listClientActiveTokens(
  clientIds: string[],
  paging: PagingModel | undefined = undefined
): Promise<TokenModel[]> {
  const payload: ClientTokenLookupRequest = { clients_list: clientIds };
  if (paging) {
    payload.paging_model = paging;
  }

  return HCLCSSFPostWithSession<TokenModel[], ClientTokenLookupRequest>(ApiRoutes.securityToken.getByClientIds, payload);
}

export async function listUserActiveTokens(
  userIds: string[],
  paging: PagingModel | undefined = undefined
): Promise<TokenModel[]> {
  const payload: UserTokenLookupRequest = { user_list: userIds };
  if (paging) {
    payload.paging_model = paging;
  }

  return HCLCSSFPostWithSession<TokenModel[], UserTokenLookupRequest>(ApiRoutes.securityToken.getByUserIds, payload);
}

export async function listTokensByDateRange(
  fromDate: string,
  toDate: string,
  includeAll = true,
  paging: PagingModel | undefined = undefined
): Promise<TokenModel[]> {
  const payload: DateRangeLookupRequest = {
    from_date: fromDate,
    to_date: toDate
  };

  if (paging) {
    payload.paging_model = paging;
  }

  const accessToken = await requireAccessToken();
  const route = includeAll ? ApiRoutes.securityToken.getAllByDateRange : ApiRoutes.securityToken.getByDateRange;
  return HCLCSSFPost<TokenModel[], DateRangeLookupRequest>(route, payload, accessToken);
}

export async function revokeToken(token: string, tokenTypeHint: "access_token" | "refresh_token") {
  const endpoint = await resolveRevocationEndpoint();
  const payload = new URLSearchParams({
    token,
    token_type_hint: tokenTypeHint
  });

  const response = await HCLCSSFFetch(endpoint, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      Authorization: getClientBasicAuthHeaderValue(),
      "X-Correlation-ID": crypto.randomUUID()
    },
    body: payload.toString(),
    cache: "no-store"
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Token revocation failed (${response.status}).`);
  }

  return true;
}

export async function bulkRevokeTokens(tokens: TokenModel[]): Promise<number> {
  let revoked = 0;
  for (const token of tokens) {
    const tokenTypeHint =
      token.TokenTypeHint === "access_token" || token.TokenTypeHint === "refresh_token"
        ? token.TokenTypeHint
        : "access_token";

    await revokeToken(token.Token, tokenTypeHint);
    revoked += 1;
  }

  return revoked;
}

export function defaultSearchWindow(): { fromDate: string; toDate: string } {
  const toDate = new Date();
  const fromDate = new Date();
  fromDate.setDate(toDate.getDate() - 30);

  return {
    fromDate: fromDate.toISOString(),
    toDate: toDate.toISOString()
  };
}

export function normalizeApiBaseUrl(): string {
  return env.apiBaseUrl;
}
