/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IntegrationTests.Endpoint.Discovery;

public class DiscoveryEndpointTests : HCLCSSFFakeSetup
{
    private const string Category = "DiscoveryEndpoint";

    public DiscoveryEndpointTests() : base("/ROOT")
    {
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Issuer_uri_should_be_lowercase()
    {
        var result = await BackChannelClient.GetAsync(DiscoveryEndpoint);

        var json = await result.Content.ReadAsStringAsync();
        var data = JObject.Parse(json);
        var issuer = data["issuer"].ToString();

        issuer.Should().Be("security.HCL.CS.SF.com");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task GetDiscoveryMetaData()
    {
        var result = await BackChannelClient.GetAsync(DiscoveryEndpoint);
        var json = await result.Content.ReadAsStringAsync();
        var data = JObject.Parse(json);
        var algorithmsSupported = data["id_token_signing_alg_values_supported"];
        var algor = algorithmsSupported.ToString();
        algor.Should().NotBeNull();
        algor.Should().Contain(SecurityAlgorithms.RsaSha256);
        algor.Should().Contain(SecurityAlgorithms.RsaSha384);
        algor.Should().Contain(SecurityAlgorithms.RsaSha512);
        algor.Should().Contain(SecurityAlgorithms.HmacSha256);
        algor.Should().Contain(SecurityAlgorithms.HmacSha384);
        algor.Should().Contain(SecurityAlgorithms.HmacSha512);
        algor.Should().Contain(SecurityAlgorithms.EcdsaSha256);
        algor.Should().Contain(SecurityAlgorithms.EcdsaSha384);
        algor.Should().Contain(SecurityAlgorithms.EcdsaSha512);
        algor.Should().Contain(SecurityAlgorithms.RsaSsaPssSha256);
        algor.Should().Contain(SecurityAlgorithms.RsaSsaPssSha384);
        algor.Should().Contain(SecurityAlgorithms.RsaSsaPssSha512);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Jwks_entries_should_contain_crv()
    {
        var result = await BackChannelClient.GetAsync(DiscoveryKeysEndpoint);

        var json = await result.Content.ReadAsStringAsync();
        var data = JObject.Parse(json);
        var keys = data["keys"];
        keys.Should().NotBeNull();
        dynamic stuff = JsonConvert.DeserializeObject(json);
        string crv = stuff.keys[0].Crv;
        crv.Should().NotBeNullOrEmpty();

        string alg = stuff.keys[0].Alg;
        alg.Should().NotBeNullOrEmpty();
    }
}
