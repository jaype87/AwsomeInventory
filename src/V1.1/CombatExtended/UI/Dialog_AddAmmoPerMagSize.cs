// <copyright file="Dialog_AddAmmoPerMagSize.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A dialog for uses choose how many ammo should be added to loadout based on weapon's magazine size.
    /// </summary>
    public class Dialog_AddAmmoPerMagSize : Window
    {
        private ThingGroupSelector _selector;
        private ThingDef _ammoUser;
        private int _magazineSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_AddAmmoPerMagSize"/> class.
        /// </summary>
        /// <param name="selector"> Selector for ammo. </param>
        /// <param name="ammoUser"> The user of ammo. </param>
        /// <param name="magazineSize"> Size of the magazine. </param>
        public Dialog_AddAmmoPerMagSize(ThingGroupSelector selector, ThingDef ammoUser, int magazineSize)
        {
            _selector = selector;
            _magazineSize = magazineSize;
            _ammoUser = ammoUser;

            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
        }

        /// <inheritdoc/>
        public override Vector2 InitialSize => new Vector2(450, 210);

        /// <inheritdoc/>
        public override void DoWindowContents(Rect inRect)
        {
            Rect labelRect = inRect.ReplaceHeight(GenUI.ListSpacing);
            Widgets.Label(labelRect, "Adjust ammo count per magazine size:");

            // Draw weapon label
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect adjustRect = labelRect.ReplaceY(labelRect.yMax);
            Widgets.Label(adjustRect, UIText.SelectedWeapon.TranslateSimple());
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(adjustRect, _ammoUser.LabelCap);

            // Draw ammo label
            Text.Anchor = TextAnchor.MiddleLeft;
            adjustRect = adjustRect.ReplaceY(adjustRect.yMax);
            Widgets.Label(adjustRect, UIText.SelectedAmmo.TranslateSimple());
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(adjustRect, _selector.AllowedThing.LabelCap);

            // Prepare to draw ammo count adjustment
            adjustRect = adjustRect.ReplaceY(adjustRect.yMax);
            float drawWidth = GenUI.SmallIconSize * 2 + UIText.TenCharsString.GetWidthCached();
            Rect centerRect = new Rect(0, adjustRect.y, drawWidth, GenUI.ListSpacing).CenteredOnXIn(adjustRect);

            // Minus sign
            Rect workingRect = centerRect.ReplaceWidth(GenUI.SmallIconSize);
            if (Widgets.ButtonImage(workingRect, TexResource.Minus))
            {
                _selector.SetStackCount(_selector.AllowedStackCount - _magazineSize);
                if (_selector.AllowedStackCount < _magazineSize)
                    _selector.SetStackCount(_magazineSize);
            }

            // Ammo count
            workingRect = workingRect.ReplaceX(workingRect.xMax).ReplaceWidth(UIText.TenCharsString.GetWidthCached());
            Widgets.Label(workingRect, _selector.AllowedStackCount.ToString());

            // Plus sign
            workingRect = workingRect.ReplaceX(workingRect.xMax).ReplaceWidth(GenUI.SmallIconSize);
            if (Widgets.ButtonImage(workingRect, TexResource.Plus))
            {
                _selector.SetStackCount(_selector.AllowedStackCount + _magazineSize);
            }

            // Cancel button
            string buttonText = UIText.CancelButton.TranslateSimple();
            workingRect = new Rect(0, workingRect.yMax + GenUI.ListSpacing, buttonText.GetWidthCached() + DrawUtility.CurrentPadding * 2, GenUI.ListSpacing);
            workingRect = workingRect.CenteredOnXIn(new Rect(0, 0, inRect.width, 0));
            if (Widgets.ButtonText(workingRect, buttonText))
            {
                this.Close();
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
