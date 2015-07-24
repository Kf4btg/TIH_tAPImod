using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;
using Terraria.ID;

namespace InvisibleHand
{
    public static class IHExtensions
    {

    #region itemExensions
        public static ItemCat GetCategory(this Item item)
        {
            foreach (ItemCat catID in Constants.CheckOrder)
            {
                if (CategoryDef.Categories[catID].Invoke(item)) return catID;
            }
            return ItemCat.OTHER;
        }

        public static bool Matches(this Item item, ItemCat isCategory)
        {
            return CategoryDef.Categories[isCategory].Invoke(item);
        }

        public static bool IsHook(this Item item)
        {
            return ProjDef.byType.ContainsKey(item.shoot) && (ProjDef.byType[item.shoot].hook || ProjDef.byType[item.shoot].aiStyle==7);
        }

        public static bool IsBomb(this Item item)
        {   //grenades, bombs, etc
            return ProjDef.byType.ContainsKey(item.shoot) && ProjDef.byType[item.shoot].aiStyle==16;
        }

        public static bool IsTool(this Item item)
        {
            return item.createTile == TileID.Rope || item.createTile == TileID.Chain || item.name.EndsWith("Bucket") ||
            item.fishingPole > 1 || item.tileWand != -1 || item.IsHook() || ItemDef.autoSelect["Glowstick"].Contains(item.type) ||
            item.type == 1991 || item.type == 50 || item.type == 1326 || ItemDef.autoSelect["Flaregun"].Contains(item.type) ||
            item.name.Contains("Paintbrush") || item.name.Contains("Paint Roller") || item.name.Contains("Paint Scraper") ||
            (item.type >= 1543 && item.type <= 1545);
            //bucket, bug net, magic mirror, rod of discord, spectre paint tools
        }
    #endregion

    #region buttonExtensions

        // public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state)
        // {
        //     if (state.texture == null)
        //     {
        //         // this creates the text-pulse effect of vanilla
        //         var textColor = Main.mouseTextColor.toScaledColor(bBase.Scale);
        //
        //         // TODO: maybe don't ignore Alpha. Also, see if it's
        //         // practical/if-there's-actually-a-need-for supporting
        //         // tinting the text (TAPI.Extensions.Multiply(textcolor, tint))
        //         sb.DrawString(
        //                 Main.fontMouseText,     //font
        //                 state.label,            //string
        //                 bBase.Position,         //position
        //                 textColor,              //color
        //                 0f,                     //rotation
        //                 default(Vector2),       //origin
        //                 bBase.Scale,            //scale
        //                 SpriteEffects.None,     //effects
        //                 0f                      //layerDepth
        //              );
        //     }
        //     else
        //         sb.Draw(state.texture, bBase.Position, bBase.SourceRect, state.tint * bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        // }
        //
        // public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state, Color overrideColor)
        // {
        //     var textColor = Main.mouseTextColor.toScaledColor(bBase.Scale);
        //
        //     if (state.texture==null)
        //         sb.DrawString(
        //                 Main.fontMouseText,     //font
        //                 state.label,            //string
        //                 bBase.Position,         //position
        //                 // overrideColor*bBase.Alpha, //color
        //                 TAPI.Extensions.Multiply(textColor, overrideColor),
        //                 0f,                     //rotation
        //                 default(Vector2),       //origin
        //                 bBase.Scale,            //scale
        //                 SpriteEffects.None,     //effects
        //                 0f                      //layerDepth
        //              );
        //     else
        //         sb.Draw(state.texture, bBase.Position, bBase.SourceRect, overrideColor*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        // }
        //
        // public static void DrawButtonBG(this SpriteBatch sb, ButtonBase bb, Texture2D bgTex, Color bgColor)
        // {
        //     sb.Draw(bgTex, bb.Position, null, bgColor*bb.Alpha, 0f, default(Vector2), bb.Scale, SpriteEffects.None, 0f);
        // }

        public static bool IsHovered(this Rectangle frame)
        {
            return frame.Contains(Main.mouseX, Main.mouseY);
        }

        /// Convert a byte to an rgba Color, using the
        /// value of the byte for each field unless a value > 1
        /// is provided for alpha. If a mult. factor is provided, each
        /// color component will be multiplied by that factor.
        public static Color toColor(this byte b, float mult = 1, float alpha = -1)
        {
            byte a;
            if (mult != 1) {
                // casting to byte is basically just taking modulus 256:
                //  (b*mult)%256
                byte c = (byte)((float)b * mult);
                a = alpha > 0 ? (byte)(alpha * mult) : c;
                return new Color((int)c, (int)c, (int)c, (int)a);
            }
            a = alpha > 0 ? (byte)alpha : b ;
            return new Color((int)b, (int)b, (int)b, (int)a);
        }

        /// This is a simplified version of toColor specifically tailored
        /// for being called each frame when drawing text-only buttons.
        /// This enables the pulse effect seen on the vanilla text.
        public static Color toScaledColor(this byte b, float mult)
        {
            var c = (int)((byte)((float)b * mult));
            return new Color(c, c, c, c);
        }

        public static Color toScaledColor(this byte b, float mult, Color tint)
        {
            var c = (int)((byte)((float)b * mult));
            return TAPI.Extensions.Multiply(new Color(c, c, c, c), tint);
            // Terraria.Utils.Multiply(textColor, tint),
        }

    #endregion

    #region TIH action exts

        /// <summary>
        /// DefaultLabelForAction
        /// </summary>
        /// <param name="action"> </param>
        /// <param name="use_originals">If true, will return the value pulled from Terraria's code for that action. </param>
        /// <returns>Corresponding label for the action or the empty string "" if one could not be found.</returns>
        public static string DefaultLabelForAction(this TIH action, bool use_originals)
        {
            if (use_originals)
            {
                switch (action)
                {
                    case TIH.LootAll:
                    case TIH.DepositAll:
                    case TIH.QuickStack:
                    case TIH.Rename:
                    case TIH.SaveName:
                    case TIH.CancelEdit:
                        return IHBase.OriginalButtonLabels[action];
                }
            }

            string label;
            if (Constants.DefaultButtonLabels.TryGetValue(action, out label))
                return label;

            return "";
        }

        /// returns the key-bind (as a string) for the button with the given action.
        /// return value will be something like "(X)"
        public static string GetKeyTip(this TIH action)
        {
            string kbopt;
            if (Constants.ButtonActionToKeyBindOption.TryGetValue(action, out kbopt))
                return IHBase.ButtonKeyTips[kbopt];

            return "";
        }

    #endregion


    /// intended to use in a fluent-interface type of way;
    /// these are generic so that a separate class so that the proper
    /// subtype will be returned rather than a generic CoreButton
    #region ButtonService helpers


        ///<summary>
        /// Add a ButtonService to this button and subscribe to its hooks
        ///</summary>
        public static T AddNewService<T>(this T button, ButtonService service) where T : ICoreButton
        {
            button.AddService(service);
            return button;
        }

        /// Activates the button's default OnClick action
        /// on_right_click = true makes the action happen
        /// on a right click rather than a left.
        public static T EnableDefault<T>(this T button, bool on_right_click = false) where T : ICoreButton
        {
            return button.AddNewService(new DefaultClickService(button, on_right_click));
        }

        public static T MakeLocking<T>(this T button, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]") where T: ICoreButton
        {
            return button.AddNewService(new LockingService(button, lock_offset, lock_color, locked_string));
        }

        public static T AddToggle<T>(this T button, T toggle_to_button, KState.Special toggle_key = KState.Special.Shift) where T: ICoreButton
        {
            return button.AddNewService(new ToggleService(button, toggle_to_button, toggle_key));
        }

        public static T AddSortToggle<T>(this T button, T reverse_button, bool sort_chest, KState.Special toggle_key = KState.Special.Shift) where T: ICoreButton
        {
            return button.AddNewService(new SortingToggleService(button, reverse_button, sort_chest, toggle_key));
        }

        public static T AddDynamicToggle<T>(this T button, T button_when_false, Func<bool> check_game_state) where T: ICoreButton
        {
            return button.AddNewService(new DynamicToggleService(button, button_when_false, check_game_state));
        }

        /// <summary>
        /// use this to help with creating buttons; e.g.:
        /// </summary>
        /// <example>
        /// <code>
        ///     TexturedButton cb = new TexturedButton(TIH.Sort).With( (b) => {
        ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        ///          b.ToolTip = "Sort Me";
        ///          // ... etc.
        ///    })
        /// </code>
        ///</example>
        public static T With<T>(this T button, Action<T> action) where T : ICoreButton
        {
            if (button != null)
                action(button);
            return button;
        }

    #endregion

    #region misc
        ///play the given sound effect;
        public static void Play(this Sound s, int x = -1, int y = -1, int style = 1)
        {
            Main.PlaySound((int)s, x, y, style);
        }

        ///<param name="min">Minimum value boundary; default is 0</param>
        ///<param name="max">Maximum value boundary; default is 1</param>
        ///<returns>value bound by specified minumum and maximum
        /// values (inclusive)</returns>
        public static float Clamp(this float value, float min = 0, float max = 1)
        {
            return MathHelper.Clamp(value, min, max);
        }

    #endregion
    }
}
