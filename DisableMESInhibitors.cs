using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace MESInhibitorInhibitor {
    public static class PatchMesInhibitors {
        
        public static void Patch(PatchContext ctx) {
            var triggerSystemType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == "ModularEncountersSystems.Behavior.Subsystems.Trigger.TriggerSystem");

            if (triggerSystemType != null) {
                var processActionMethod = triggerSystemType.GetMethod("ProcessAction", BindingFlags.Instance | BindingFlags.Public);
                if (processActionMethod != null) {
                    ctx.GetPattern(processActionMethod).Prefixes.Add(typeof(PatchMesInhibitors).GetMethod(nameof(ProcessActionPrefix), BindingFlags.Static | BindingFlags.NonPublic));
                }
            }
            
            var armorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => {
                    try { return asm.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == "ModularEncountersSystems.Spawning.Manipulation.ArmorModuleReplacement");

            if (armorType == null) {
                MyLog.Default.WriteLineAndConsole("[MESInhibitorInhibitor] Could not find ArmorModuleReplacement type.");
                return;
            }

            var replaceMethod = armorType.GetMethod(
                "ReplaceArmorWithModule",
                BindingFlags.Public | BindingFlags.Static
            );

            if (replaceMethod != null) {
                ctx.GetPattern(replaceMethod)
                    .Prefixes.Add(typeof(PatchMesInhibitors)
                    .GetMethod(nameof(ReplaceArmorWithModulePrefix), BindingFlags.Static | BindingFlags.NonPublic));
            } else {
                MyLog.Default.WriteLineAndConsole("[MESInhibitorInhibitor] Could not find ReplaceArmorWithModule method.");
            }

            var setRangeMethod = armorType.GetMethod(
                "SetDefaultInhibitorRanges",
                BindingFlags.Public | BindingFlags.Static
            );

            if (setRangeMethod != null) {
                ctx.GetPattern(setRangeMethod)
                    .Prefixes.Add(typeof(PatchMesInhibitors)
                    .GetMethod(nameof(SetDefaultInhibitorRangesPrefix), BindingFlags.Static | BindingFlags.NonPublic));
            } else {
                MyLog.Default.WriteLineAndConsole("[MESInhibitorInhibitor] Could not find SetDefaultInhibitorRanges method.");
            }

            string baseNamespace = "ModularEncountersSystems.BlockLogic";
            string[] inhibitorTypes = {
                "InhibitorLogic",
                "DrillInhibitor",
                "JumpDriveInhibitor",
                "NanobotInhibitor",
                "JetpackInhibitor",
                "EnergyInhibitor",
                "PlayerInhibitor"
            };

            foreach (var inhibitorName in inhibitorTypes) {
                string fullTypeName = $"{baseNamespace}.{inhibitorName}";
                PatchDisableAllMethods(ctx, fullTypeName);
            }
            
        }
        
        private static bool ProcessActionPrefix(object trigger, object actionsBase, long attackerEntityId, long detectedEntity, object command) {
            var actionProfile = actionsBase as dynamic;
            if (actionProfile == null)
                return true;

            var actions = actionProfile.ActionReference;

            try {
                // Log all relevant property values to see what is really there, sometimes it goes for a walk :/
                bool generateExplosion = false;
                bool damageToolAttacker = false;
                bool jetpackInhibitorEffect = false;
                bool drillInhibitorEffect = false;
                bool nanobotInhibitorEffect = false;
                bool jumpInhibitorEffect = false;
                bool playerInhibitorEffect = false;

                try { generateExplosion = actions.GenerateExplosion == true; } catch {}
                try { damageToolAttacker = actions.DamageToolAttacker == true; } catch {}
                try { jetpackInhibitorEffect = actions.UseJetpackInhibitorEffect == true; } catch {}
                try { drillInhibitorEffect = actions.UseDrillInhibitorEffect == true; } catch {}
                try { nanobotInhibitorEffect = actions.UseNanobotInhibitorEffect == true; } catch {}
                try { jumpInhibitorEffect = actions.UseJumpInhibitorEffect == true; } catch {}
                try { playerInhibitorEffect = actions.UsePlayerInhibitorEffect == true; } catch {}

                // MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Action flags - GenerateExplosion: {generateExplosion}, " +
                //                                   $"DamageToolAttacker: {damageToolAttacker}, " +
                //                                   $"JetpackInhibitor: {jetpackInhibitorEffect}, " +
                //                                   $"DrillInhibitor: {drillInhibitorEffect}, " +
                //                                   $"NanobotInhibitor: {nanobotInhibitorEffect}, " +
                //                                   $"JumpInhibitor: {jumpInhibitorEffect}, " +
                //                                   $"PlayerInhibitor: {playerInhibitorEffect}");

                if (generateExplosion || damageToolAttacker || jetpackInhibitorEffect || drillInhibitorEffect || nanobotInhibitorEffect || jumpInhibitorEffect || playerInhibitorEffect) {
                    // MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Blocked action: {actionProfile.ProfileSubtypeId}");
                    return false;
                }
            }
            catch (Exception ex) {
                MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Exception in ProcessActionPrefix: {ex}");
                return true;
            }

            return true;
        }

        private static void PatchDisableAllMethods(PatchContext ctx, string fullTypeName) {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => {
                    try { return asm.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => t.FullName == fullTypeName);

            if (type == null) {
                MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Could not find type: {fullTypeName}");
                return;
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.DeclaringType == type);

            foreach (var method in methods) {
                if (method.IsSpecialName || method.IsConstructor)
                    continue;

                ctx.GetPattern(method)
                    .Prefixes.Add(typeof(PatchMesInhibitors)
                        .GetMethod(nameof(EmptyPrefix), BindingFlags.Static | BindingFlags.NonPublic));
            }
            MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Disabled all instance methods on {fullTypeName}");
        }

        private static bool EmptyPrefix() {
            return false;
        }

        private static bool ReplaceArmorWithModulePrefix(
            List<MyObjectBuilder_CubeBlock> blocks,
            MyObjectBuilder_CubeBlock oldBlock,
            SerializableDefinitionId newBlockId,
            object data)
        {
            var subtype = newBlockId.SubtypeName ?? string.Empty;

            if (!subtype.Contains("MES-Suppressor-"))
                return true;

            MyLog.Default.WriteLineAndConsole(
                $"[MESInhibitorInhibitor] Prevented placement of MES inhibitor: {subtype}"
            );
            return false;
        }


        private static bool SetDefaultInhibitorRangesPrefix(object block, object data) {
            var subtypeProp = block?.GetType().GetProperty("SubtypeName", BindingFlags.Public | BindingFlags.Instance);
            var subtype = subtypeProp?.GetValue(block) as string ?? string.Empty;

            if (!subtype.StartsWith("MES-Suppressor-", StringComparison.Ordinal)) return true;

            var radiusProp = block.GetType().GetProperty("BroadcastRadius", BindingFlags.Public | BindingFlags.Instance);
            if (radiusProp != null && radiusProp.CanWrite) {
                radiusProp.SetValue(block, 0f);
            }

            MyLog.Default.WriteLineAndConsole($"[MESInhibitorInhibitor] Neutralized inhibitor '{subtype}' (radius set to 0).");
            return false;
        }
    }
}
