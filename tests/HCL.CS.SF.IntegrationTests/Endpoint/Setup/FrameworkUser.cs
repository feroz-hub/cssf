/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using IdentityModel;

namespace IntegrationTests.Endpoint.Setup;

public class FrameworkUser
{
    public FrameworkUser(string subjectId, string userName)
    {
        if (string.IsNullOrEmpty(subjectId)) throw new ArgumentException("SubjectId is mandatory", nameof(subjectId));

        SubjectId = subjectId;
        if (string.IsNullOrEmpty(userName)) throw new ArgumentException("User Name is mandatory", nameof(userName));
        UserName = userName;
    }

    public string SubjectId { get; }

    public string UserName { get; }

    public string DisplayName { get; set; }

    public string IdentityProvider { get; set; }

    public ICollection<string> AuthenticationMethods { get; set; } = new HashSet<string>();

    public DateTime? AuthenticationTime { get; set; }

    public ICollection<Claim> AdditionalClaims { get; set; } = new HashSet<Claim>(new ClaimComparer());

    public ClaimsPrincipal CreatePrincipal()
    {
        if (string.IsNullOrEmpty(SubjectId)) throw new ArgumentException("Subject Id is mandatory");

        var claims = new List<Claim> { new(JwtClaimTypes.Subject, SubjectId) };
        claims.Add(new Claim(JwtClaimTypes.Name, UserName));

        //if (!string.IsNullOrEmpty(DisplayName))
        //{
        //    claims.Add(new Claim(JwtClaimTypes.Name, DisplayName));
        //}

        if (!string.IsNullOrEmpty(IdentityProvider))
            claims.Add(new Claim(JwtClaimTypes.IdentityProvider, IdentityProvider));

        if (AuthenticationTime.HasValue)
            claims.Add(new Claim(JwtClaimTypes.AuthenticationTime,
                new DateTimeOffset(AuthenticationTime.Value).ToUnixTimeSeconds().ToString()));

        if (AuthenticationMethods.Any())
            foreach (var amr in AuthenticationMethods)
                claims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, amr));

        claims.AddRange(AdditionalClaims);

        var id = new ClaimsIdentity(claims.Distinct(new ClaimComparer()), "HCL.CS.SF", JwtClaimTypes.Name,
            JwtClaimTypes.Role);

        return new ClaimsPrincipal(id);
    }
}
