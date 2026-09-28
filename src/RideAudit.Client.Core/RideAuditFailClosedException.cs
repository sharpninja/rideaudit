// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

namespace RideAudit.Client.Core;

/// <summary>
/// Fail-closed rejection. Callers must not continue keygen, seal, upload, decrypt, or display.
/// </summary>
public sealed class RideAuditFailClosedException : Exception
{
    public RideAuditFailClosedException(string code, string requirementId, string message)
        : base(message)
    {
        Code = code;
        RequirementId = requirementId;
    }

    public string Code { get; }

    public string RequirementId { get; }
}
