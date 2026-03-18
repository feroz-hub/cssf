/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Newtonsoft.Json;
using HCL.CS.SF.Domain.Constants.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Converters;

/// <summary>
/// Custom JSON converter for serializing/deserializing <see cref="ClaimsPrincipal"/> objects.
/// Used when persisting authorization codes that contain the authenticated user's claims principal.
/// Converts between ClaimsPrincipal and a flat JSON representation of claims and authentication type.
/// </summary>
internal class ClaimsPrincipalConverter : JsonConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(ClaimsPrincipal) == objectType;
    }

    /// <summary>
    /// Reads the jso.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="objectType">The object type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The serializer.</param>
    /// <returns>The operation result.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var source = serializer.Deserialize<ClaimsPrincipalObject>(reader);
        if (source == null) return null;

        var claims = source.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType));
        var id = new ClaimsIdentity(claims, source.AuthenticationType, OpenIdConstants.ClaimTypes.Name,
            OpenIdConstants.ClaimTypes.Role);
        var target = new ClaimsPrincipal(id);
        return target;
    }

    /// <summary>
    /// Writes the jso.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var source = (ClaimsPrincipal)value;

        var target = new ClaimsPrincipalObject
        {
            AuthenticationType = source.Identity.AuthenticationType,
            Claims = source.Claims.Select(x => new ClaimObject
                { Type = x.Type, Value = x.Value, ValueType = x.ValueType }).ToArray()
        };
        serializer.Serialize(writer, target);
    }
}

/// <summary>
/// Intermediate DTO for JSON serialization of a <see cref="ClaimsPrincipal"/>.
/// </summary>
public class ClaimsPrincipalObject
{
    /// <summary>Gets or sets the authentication type string for the claims identity.</summary>
    public string AuthenticationType { get; set; }

    /// <summary>Gets or sets the array of serialized claim objects.</summary>
    public ClaimObject[] Claims { get; set; }
}
