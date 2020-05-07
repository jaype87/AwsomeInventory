// <copyright file="Pawn_InventoryTracker_FirstUnloadableThing_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using HarmonyLib;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Patch FirstUnloadableThing so that it skips items in loadout.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_InventoryTracker_FirstUnloadableThing_Patch
    {
        private static List<CodeInstruction> _headPattern;
        private static List<CodeInstruction> _startPattern;
        private static MethodInfo _checkThingInLoadout;

        static Pawn_InventoryTracker_FirstUnloadableThing_Patch()
        {
            _headPattern = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Blt),
                new CodeInstruction(OpCodes.Callvirt),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Stloc_3),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldloc_3),
            };
            _startPattern = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Stloc_3),
                new CodeInstruction(OpCodes.Br),
            };
            _checkThingInLoadout = AccessTools.Method(typeof(Pawn_InventoryTracker_FirstUnloadableThing_Patch), "ThingInLoadout");

            MethodInfo original = AccessTools.PropertyGetter(typeof(Pawn_InventoryTracker), "FirstUnloadableThing");
            MethodInfo transpiler = AccessTools.Method(typeof(Pawn_InventoryTracker_FirstUnloadableThing_Patch), "Transpiler");
            Utility.Harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
        }

        /// <summary>
        /// Patch the IL code in <see cref="Pawn_InventoryTracker.FirstUnloadableThing"/>, so that it ignores things in loadout.
        /// </summary>
        /// <param name="instructions"> IL insctructions of the method.</param>
        /// <param name="iLGenerator"> Generator for IL instruction. </param>
        /// <returns> A list of instruction. </returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony patch")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            /* <Before>
                    * for (int j = 0; j < this.innerContainer.Count; j++)
                    * {
                    *       if (!this.innerContainer[j].def.IsDrug)
                    *      {
                    *            return new ThingCount(this.innerContainer[j], this.innerContainer[j].stackCount);
                    *       }
                    *  ......
                    * }
                    *
                    * <After>
                    * for (int j = 0; j < this.innerContainer.Count; j++)
                    * {
                    *       if (ThingInLoadout(this, j)
                    *            continue;
                    *
                    *       if (!this.innerContainer[j].def.IsDrug)
                    *      {
                    *            return new ThingCount(this.innerContainer[j], this.innerContainer[j].stackCount);
                    *       }
                    *  ......
                    * }
                    */
            Label startLabel = iLGenerator.DefineLabel();
            Label continueLabel = iLGenerator.DefineLabel();
            List<CodeInstruction> invert = new List<CodeInstruction>(instructions);
            invert.Reverse();

            // Match the pattern.
            for (int i = 0; i < invert.Count; i++)
            {
                if (invert[i].opcode == _headPattern[0].opcode)
                {
                    int k = 1;
                    bool matched = true;
                    for (int j = i + k; k < _headPattern.Count; j++, k++)
                    {
                        if (invert[j].opcode != _headPattern[k].opcode)
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        // Assign a new label operand for instruction Blt.
                        invert[i].operand = startLabel;

                        // Set up a continue label for later use, equivalent of the continue keyword.
                        invert[i + k - 1].labels.Add(continueLabel);
                    }
                }
            }

            bool found = false;
            bool patched = false;
            int m = 0;

            List<CodeInstruction> original = new List<CodeInstruction>(instructions);
            for (int i = 0; i < original.Count; i++)
            {
                yield return original[i];
                if (patched == false && original[i].opcode == _startPattern[m].opcode)
                {
                    ++m;
                    if (m == _startPattern.Count)
                        found = true;
                }
                else
                {
                    m = 0;
                }

                if (found)
                {
                    CodeInstruction newLoopStart = new CodeInstruction(OpCodes.Ldarg_0);
                    newLoopStart.labels.Add(startLabel);

                    yield return newLoopStart;
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, _checkThingInLoadout);
                    yield return new CodeInstruction(OpCodes.Brtrue, continueLabel);

                    patched = true;
                    found = false;
                }
            }
        }

        private static bool ThingInLoadout(Pawn_InventoryTracker inventory, int index)
        {
            CompAwesomeInventoryLoadout comp = inventory?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp?.Loadout != null)
            {
                Thing thingToUnload = inventory.innerContainer[index];
                foreach (ThingGroupSelector groupSelector in comp.Loadout)
                {
                    if (groupSelector.Allows(thingToUnload, out _) && !(comp.InventoryMargins[groupSelector] > 0))
                        return true;
                }

                if (SimpleSidearmUtility.IsActive)
                {
                    return SimpleSidearmUtility.InMemory(inventory.pawn, inventory.innerContainer[index]);
                }

                return false;
            }
            else if (comp != null)
            {
                if (comp.InventoryMargins != null)
                {
                    Log.Error("Loadout and InventoryMargins are out of sync. This message is harmless. Resetting");
                    comp.RemoveLoadout();
                }
            }

            return false;
        }
    }
}
