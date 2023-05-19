﻿// Copyright 2023 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

#if NET6_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

namespace Nuke.Common.Tooling;

[PublicAPI]
[InterpolatedStringHandler]
public ref struct ArgumentStringHandler
{
    private DefaultInterpolatedStringHandler _builder;
    private readonly List<string> _secretValues;

    public ArgumentStringHandler(
        int literalLength,
        int formattedCount,
        out bool handlerIsValid)
    {
        _builder = new(literalLength, formattedCount);
        _secretValues = new List<string>();
        handlerIsValid = true;
    }

    public static implicit operator ArgumentStringHandler(string value)
    {
        return $"{value.NotNull()}";
    }

    public void AppendLiteral(string value)
    {
        _builder.AppendLiteral(value);
    }

    public void AppendFormatted(object obj, int alignment = 0, string format = null)
    {
        AppendFormatted(obj.ToString(), alignment, format);
    }

    public void AppendFormatted(string value, int alignment = 0, string format = null)
    {
        if (format == "r")
            _secretValues.Add(value);
        else if (!(value.IsDoubleQuoted() || value.IsSingleQuoted() || format == "nq"))
            (value, format) = (value.DoubleQuoteIfNeeded(), null);

        _builder.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(AbsolutePath path, int alignment = 0, string format = null)
    {
        _builder.AppendFormatted(path, alignment, format ?? "dn");
    }

    public string ToStringAndClear()
    {
        return _builder.ToStringAndClear().TrimMatchingDoubleQuotes();
    }

    public Func<string, string> GetFilter()
    {
        var secretValues = _secretValues;
        return x => secretValues.Aggregate(x, (arguments, value) => arguments.Replace(value, Arguments.Redacted));
    }
}
#endif
