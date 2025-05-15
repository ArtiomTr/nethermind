// SPDX-FileCopyrightText: 2024 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Logging;
using Grandine.Bindings;

namespace Nethermind.Grandine;

public class GrandinePlugin(IGrandineConfig grandineConfig) : INethermindPlugin
{
    public string Name => "Grandine plugin";
    public string Description => "Nethermind plugin to enable embedded grandine CL client";
    public string Author => "Grandine team";

    private ILogger logger;

    public bool Enabled => grandineConfig.Enabled;

    public Task Init(INethermindApi nethermindApi)
    {

        logger = nethermindApi.LogManager.GetClassLogger();
        logger.Debug("Initializing grandine plugin...");

        Type configType = grandineConfig.GetType();

        PropertyInfo[] properties = configType.GetInterface("IGrandineConfig").GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var arguments = new List<string>();

        foreach (PropertyInfo prop in properties)
        {
            GrandineConfigItemAttribute attribute = prop.GetCustomAttribute<GrandineConfigItemAttribute>();

            if (attribute == null)
            {
                continue;
            }

            object value = prop.GetValue(grandineConfig);

            if (value == null)
            {
                continue;
            }

            if (prop.PropertyType == typeof(bool) && (bool)value)
            {
                arguments.Add(attribute.Name);
            }
            else if (prop.PropertyType == typeof(string))
            {
                arguments.Add(attribute.Name);
                arguments.Add((string)value);
            }
            else if (prop.PropertyType.IsArray)
            {
                var elementType = prop.PropertyType.GetElementType();
                if (elementType == typeof(string))
                {
                    var values = (string[])value;

                    foreach (var val in values)
                    {
                        arguments.Add(attribute.Name);
                        arguments.Add(val);
                    }
                }
            }
        }

        logger.Debug($"Starting grandine with arguments: {string.Join(", ", arguments)}");

        Task task = new Task(() =>
        {
            var client = new GrandineClient();
            try
            {
                client.Run(arguments.ToArray());
            }
            catch (Exception e)
            {
                logger.Error($"Failed to start grandine: {e}");
            }
        });
        task.Start();

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() { return ValueTask.CompletedTask; }
}
