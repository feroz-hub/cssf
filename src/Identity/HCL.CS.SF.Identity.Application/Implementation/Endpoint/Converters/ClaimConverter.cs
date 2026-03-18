/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Newtonsoft.Json;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Converters;

/// <summary>
/// Custom JSON converter for serializing/deserializing <see cref="Claim"/> objects.
/// Used alongside <see cref="ClaimsPrincipalConverter"/> for authorization code persistence.
/// </summary>
internal class ClaimConverter : JsonConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(Claim) == objectType;
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
        var source = serializer.Deserialize<ClaimObject>(reader);
        var target = new Claim(source.Type, source.Value, source.ValueType);
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
        var source = (Claim)value;

        var target = new ClaimObject
        {
            Type = source.Type,
            Value = source.Value,
            ValueType = source.ValueType
        };

        serializer.Serialize(writer, target);
    }
}

/// <summary>
/// Intermediate DTO for JSON serialization of a <see cref="Claim"/>.
/// </summary>
public class ClaimObject
{
    /// <summary>Gets or sets the claim type URI (e.g., "sub", "name", "role").</summary>
    public string Type { get; set; }

    /// <summary>Gets or sets the claim value.</summary>
    public string Value { get; set; }

    /// <summary>Gets or sets the claim value type URI (e.g., ClaimValueTypes.String).</summary>
    public string ValueType { get; set; }
}
