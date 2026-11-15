using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    /// <summary>t1454 / t1457: pixel pivots for the PixelLab batch 1-3 strips (art notes, d2feel_t1447_pixellab). Hit / death strips use pivot x = w/2 and the sheet's ground row; corpses are trimmed images whose pivot = death pivot minus the crop origin (table below, measured by matching each corpse against the last death frame, exact pixel match).</summary>
    public static class D2Pl1454
    {
        /// <summary>Pixels above the frame bottom for the pivot (ground row): 1 for the batch 1 / brute / archer slots, 5 for the 38x32 Mortar / Frost Caster cells, 4 for the 40x40 Fire Wheel cell, boss cells = SheetDef pivot (Ox-Head 6, Horse-Face 8, Ice Gorilla 6, Qilin 8).</summary>
        public static float GroundPx(string key)
        {
            switch (key)
            {
                case "frost_caster": case "rice_ghost": case "grinder_monk": case "bloater": return 5f; case "fire_wheel": return 4f;
                case "boss_oxhead_clubjailer": case "boss_ice_gorilla": return 6f; case "boss_horseface_pestle": case "boss_qilin_chariot": return 8f;
                default: return 1f;
            }
        }
        /// <summary>Ghost bodies (also their recolours wraith_v2_k4 / k5): ectoplasm pool, no red blood decal / death burst.</summary>
        public static bool IsGhost(string key) => key.StartsWith("wraith_v2") || key == "rice_ghost";
        /// <summary>Corpse pivot in pixels from the top-left of the trimmed image (rel = Enemies/&lt;key&gt;_corpse_&lt;dir&gt;_pl).</summary>
        public static bool TryCorpse(string rel, out Vector2 pxFromTopLeft) => Corpse.TryGetValue(rel, out pxFromTopLeft);
        static readonly Dictionary<string, Vector2> Corpse = new Dictionary<string, Vector2>
        {
            { "Enemies/archer_corpse_east_pl", new Vector2(17.0f, 17f) },
            { "Enemies/archer_corpse_north_pl", new Vector2(19.0f, 22f) },
            { "Enemies/archer_corpse_south_pl", new Vector2(18.0f, 21f) },
            { "Enemies/archer_corpse_west_pl", new Vector2(19.0f, 17f) },
            { "Enemies/bloater_corpse_east_pl", new Vector2(19.0f, 11f) },
            { "Enemies/bloater_corpse_north_pl", new Vector2(19.0f, 11f) },
            { "Enemies/bloater_corpse_south_pl", new Vector2(19.0f, 11f) },
            { "Enemies/bloater_corpse_west_pl", new Vector2(7.0f, 11f) },
            { "Enemies/boss_horseface_pestle_corpse_southeast_pl", new Vector2(35.0f, 45f) },
            { "Enemies/boss_ice_gorilla_corpse_south_pl", new Vector2(28.0f, 44f) },
            { "Enemies/boss_oxhead_clubjailer_corpse_south_pl", new Vector2(36.0f, 49f) },
            { "Enemies/boss_qilin_chariot_corpse_east_pl", new Vector2(65.0f, 89f) },
            { "Enemies/boss_qilin_chariot_corpse_south_pl", new Vector2(64.0f, 96f) },
            { "Enemies/brute_corpse_east_pl", new Vector2(21.5f, 19f) },
            { "Enemies/brute_corpse_north_pl", new Vector2(22.5f, 27f) },
            { "Enemies/brute_corpse_south_pl", new Vector2(22.5f, 25f) },
            { "Enemies/brute_corpse_west_pl", new Vector2(24.5f, 19f) },
            { "Enemies/brute_var1_corpse_east_pl", new Vector2(21.5f, 19f) },
            { "Enemies/brute_var1_corpse_north_pl", new Vector2(22.5f, 27f) },
            { "Enemies/brute_var1_corpse_south_pl", new Vector2(22.5f, 25f) },
            { "Enemies/brute_var1_corpse_west_pl", new Vector2(24.5f, 19f) },
            { "Enemies/fire_wheel_corpse_east_pl", new Vector2(20.0f, 14f) },
            { "Enemies/fire_wheel_corpse_west_pl", new Vector2(16.0f, 14f) },
            { "Enemies/frost_brute_corpse_east_pl", new Vector2(21.5f, 19f) },
            { "Enemies/frost_brute_corpse_north_pl", new Vector2(22.5f, 27f) },
            { "Enemies/frost_brute_corpse_south_pl", new Vector2(22.5f, 25f) },
            { "Enemies/frost_brute_corpse_west_pl", new Vector2(24.5f, 19f) },
            { "Enemies/frost_caster_corpse_east_pl", new Vector2(15.0f, 22f) },
            { "Enemies/frost_caster_corpse_north_pl", new Vector2(16.0f, 23f) },
            { "Enemies/frost_caster_corpse_south_pl", new Vector2(17.0f, 24f) },
            { "Enemies/frost_caster_corpse_west_pl", new Vector2(15.0f, 22f) },
            { "Enemies/gossip_imp_corpse_east_pl", new Vector2(12.0f, 16f) },
            { "Enemies/gossip_imp_corpse_north_pl", new Vector2(11.0f, 20f) },
            { "Enemies/gossip_imp_corpse_south_pl", new Vector2(9.0f, 18f) },
            { "Enemies/gossip_imp_corpse_west_pl", new Vector2(12.0f, 16f) },
            { "Enemies/grinder_monk_corpse_east_pl", new Vector2(13.0f, 19f) },
            { "Enemies/grinder_monk_corpse_north_pl", new Vector2(15.0f, 23f) },
            { "Enemies/grinder_monk_corpse_south_pl", new Vector2(13.0f, 24f) },
            { "Enemies/grinder_monk_corpse_west_pl", new Vector2(14.0f, 19f) },
            { "Enemies/hook_thrower_corpse_east_pl", new Vector2(16.0f, 18f) },
            { "Enemies/hook_thrower_corpse_north_pl", new Vector2(15.0f, 18f) },
            { "Enemies/hook_thrower_corpse_south_pl", new Vector2(16.0f, 23f) },
            { "Enemies/hook_thrower_corpse_west_pl", new Vector2(17.0f, 18f) },
            { "Enemies/imp_v2_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k1_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k1_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_k1_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_k1_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k2_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k2_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_k2_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_k2_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k3_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k3_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_k3_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_k3_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k7_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_k7_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_k7_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_k7_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_var1_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_var1_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_var1_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_var1_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_var2_corpse_east_pl", new Vector2(18.5f, 15f) },
            { "Enemies/imp_v2_var2_corpse_north_pl", new Vector2(18.5f, 20f) },
            { "Enemies/imp_v2_var2_corpse_south_pl", new Vector2(18.5f, 26f) },
            { "Enemies/imp_v2_var2_corpse_west_pl", new Vector2(18.5f, 15f) },
            { "Enemies/rice_ghost_corpse_east_pl", new Vector2(19.0f, 20f) },
            { "Enemies/rice_ghost_corpse_north_pl", new Vector2(14.0f, 19f) },
            { "Enemies/rice_ghost_corpse_south_pl", new Vector2(14.0f, 22f) },
            { "Enemies/rice_ghost_corpse_west_pl", new Vector2(19.0f, 20f) },
            { "Enemies/shambler_v2_corpse_east_pl", new Vector2(18.0f, 17f) },
            { "Enemies/shambler_v2_corpse_north_pl", new Vector2(19.0f, 21f) },
            { "Enemies/shambler_v2_corpse_south_pl", new Vector2(19.0f, 24f) },
            { "Enemies/shambler_v2_corpse_west_pl", new Vector2(19.0f, 17f) },
            { "Enemies/shambler_v2_var1_corpse_east_pl", new Vector2(18.0f, 17f) },
            { "Enemies/shambler_v2_var1_corpse_north_pl", new Vector2(19.0f, 21f) },
            { "Enemies/shambler_v2_var1_corpse_south_pl", new Vector2(19.0f, 24f) },
            { "Enemies/shambler_v2_var1_corpse_west_pl", new Vector2(19.0f, 17f) },
            { "Enemies/shambler_v2_var2_corpse_east_pl", new Vector2(18.0f, 17f) },
            { "Enemies/shambler_v2_var2_corpse_north_pl", new Vector2(19.0f, 21f) },
            { "Enemies/shambler_v2_var2_corpse_south_pl", new Vector2(19.0f, 24f) },
            { "Enemies/shambler_v2_var2_corpse_west_pl", new Vector2(19.0f, 17f) },
            { "Enemies/split_child_corpse_east_pl", new Vector2(13.5f, 6f) },
            { "Enemies/split_child_corpse_north_pl", new Vector2(13.5f, 7f) },
            { "Enemies/split_child_corpse_south_pl", new Vector2(13.5f, 6f) },
            { "Enemies/split_child_corpse_west_pl", new Vector2(6.5f, 6f) },
            { "Enemies/splitter_corpse_east_pl", new Vector2(25.0f, 10f) },
            { "Enemies/splitter_corpse_north_pl", new Vector2(24.0f, 9f) },
            { "Enemies/splitter_corpse_south_pl", new Vector2(23.0f, 10f) },
            { "Enemies/splitter_corpse_west_pl", new Vector2(14.0f, 10f) },
            { "Enemies/splitter_var1_corpse_east_pl", new Vector2(25.0f, 10f) },
            { "Enemies/splitter_var1_corpse_north_pl", new Vector2(24.0f, 9f) },
            { "Enemies/splitter_var1_corpse_south_pl", new Vector2(23.0f, 10f) },
            { "Enemies/splitter_var1_corpse_west_pl", new Vector2(14.0f, 10f) },
            { "Enemies/wraith_v2_corpse_east_pl", new Vector2(16.0f, 18f) },
            { "Enemies/wraith_v2_corpse_north_pl", new Vector2(16.0f, 26f) },
            { "Enemies/wraith_v2_corpse_south_pl", new Vector2(16.0f, 19f) },
            { "Enemies/wraith_v2_corpse_west_pl", new Vector2(17.0f, 18f) },
            { "Enemies/wraith_v2_k4_corpse_east_pl", new Vector2(16.0f, 18f) },
            { "Enemies/wraith_v2_k4_corpse_north_pl", new Vector2(16.0f, 26f) },
            { "Enemies/wraith_v2_k4_corpse_south_pl", new Vector2(16.0f, 19f) },
            { "Enemies/wraith_v2_k4_corpse_west_pl", new Vector2(17.0f, 18f) },
            { "Enemies/wraith_v2_k5_corpse_east_pl", new Vector2(16.0f, 18f) },
            { "Enemies/wraith_v2_k5_corpse_north_pl", new Vector2(16.0f, 26f) },
            { "Enemies/wraith_v2_k5_corpse_south_pl", new Vector2(16.0f, 19f) },
            { "Enemies/wraith_v2_k5_corpse_west_pl", new Vector2(17.0f, 18f) }
        };
    }
}
