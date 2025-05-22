// SPDX-FileCopyrightText: 2024 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;

namespace Nethermind.Grandine;

public class GrandineConfigItemAttribute : Attribute
{
    public required string Name { get; set; }
}
