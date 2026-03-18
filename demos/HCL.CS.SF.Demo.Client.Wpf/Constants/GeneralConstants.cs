/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoClientWpfApp.Constants
{
     public static class Algorithms
    {
        public const string EcdsaSha256 = "ES256";

        public const string EcdsaSha384 = "ES384";

        public const string EcdsaSha512 = "ES512";
        public const string HmacSha384 = "HS384";
        public const string HmacSha512 = "HS512";
        public const string HmacSha256 = "HS256";

        public const string RsaSha256 = "RS256";
        public const string RsaSha384 = "RS384";
        public const string RsaSha512 = "RS512";
        public const string RsaSsaPssSha256 = "PS256";
        public const string RsaSsaPssSha384 = "PS384";
        public const string RsaSsaPssSha512 = "PS512";
    }

    public static class GrantTypes
    {
        public const string Password = "password";
        public const string AuthorizationCode = "authorization_code";
        public const string ClientCredentials = "client_credentials";
        public const string RefreshToken = "refresh_token";
    }
    public static class AuthenticationConstants
    {
        public const int KeySize16 = 16;
        public const int KeySize24 = 24;
        public const int KeySize32 = 32;
    }

    public static class AuthorizeRequest
    {
        public const string Scope = "scope";
        public const string CodeChallengeMethod = "code_challenge_method";
        public const string CodeChallenge = "code_challenge";
        public const string MaxAge = "max_age";
        public const string Prompt = "prompt";
        public const string Nonce = "nonce";
        public const string ResponseMode = "response_mode";
        public const string State = "state";
        public const string RedirectUri = "redirect_uri";
        public const string ClientId = "client_id";
        public const string ResponseType = "response_type";
        public const string Request = "request";
        public const string RequestUri = "request_uri";
        public const string Audience = "aud";
    }

    public static class TokenRequest
    {
        public const string GrantType = "grant_type";
        public const string RedirectUri = "redirect_uri";
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string ClientAssertion = "client_assertion";
        public const string ClientAssertionType = "client_assertion_type";
        public const string Assertion = "assertion";
        public const string Code = "code";
        public const string RefreshToken = "refresh_token";
        public const string Scope = "scope";
        public const string UserName = "username";
        public const string UserId = "userid";
        public const string Password = "password";
        public const string CodeVerifier = "code_verifier";
        public const string TokenType = "token_type";
        public const string Algorithm = "alg";
        public const string Key = "key";
        public const string Token = "token";
        // token exchange
        public const string Resource = "resource";
        public const string Audience = "audience";
    }

    public static class IntrospectionRequest
    {
        public const string Token = "token";
        public const string Scope = "scope";
        public const string TokenHintType = "token_hint_type";
    }

    public static class AuthenticationSchemes
    {
        public const string AuthorizationHeaderBearer = "Bearer";
    }

    public static class GlobalConfiguration
    {
        public static bool IsEmailConfigurationValid { get; set; } = false;

        public static bool IsSmsConfigurationValid { get; set; } = false;

        public static bool IsLdapConfigurationValid { get; set; } = false;
    }
}


