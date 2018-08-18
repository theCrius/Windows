using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace WindowMod
{

    public class Building_Window : Building
    {

        public float WindowViewBeauty;

        private CellRect skyRect;
        private Building glower;
        private CompGlower glowComp;
        private float skyLight = 0;
        private WindowManager mgr;
        private static ColorInt darkColor = new ColorInt(0, 0, 0, 0);

        public IntVec3 ViewCell
        {
            get
            {
                return Position + Rotation.FacingCell;
            }
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            mgr = map.GetComponent<WindowManager>();
            if (mgr == null)
            {
                MapComponent comp = (MapComponent)Activator.CreateInstance(typeof(WindowManager), new object[] { map });
                map.components.Add(comp);
                mgr = map.GetComponent<WindowManager>();
            }

            // Assign position info based on this window's rotation
            AssignWindowSkyArea();

            // Spawn the glower
            glower = SpawnGlower(ViewCell);
        }


        private static CellRect CellRectNorth(IntVec3 root)
        {
            return new CellRect()
            {
                minX = root.x - 1,
                minZ = root.z + 1,
                maxX = root.x + 1,
                maxZ = root.z + 5
            };
        }


        private static CellRect CellRectSouth(IntVec3 root)
        {
            return new CellRect()
            {
                minX = root.x - 1,
                minZ = root.z - 5,
                maxX = root.x + 1,
                maxZ = root.z - 1
            };
        }


        private static CellRect CellRectEast(IntVec3 root)
        {
            return new CellRect()
            {
                minX = root.x + 1,
                minZ = root.z - 1,
                maxX = root.x + 5,
                maxZ = root.z + 1
            };
        }


        private static CellRect CellRectWest(IntVec3 root)
        {
            return new CellRect()
            {
                minX = root.x - 5,
                minZ = root.z - 1,
                maxX = root.x - 1,
                maxZ = root.z + 1
            };
        }


        private Building SpawnGlower(IntVec3 pos)
        {
            // Create the glower
            Building glower = ThingMaker.MakeThing(LocalDefOf.WIN_WindowGlower) as Building;

            // Spawn the glower
            GenSpawn.Spawn(glower, pos, Map);
            glowComp = glower.GetComp<CompGlower>();
            if (glowComp == null)
            {
                glowComp = new CompGlower();
                glowComp.parent = glower;
                glower.AllComps.Add(glowComp);
                glowComp.Props.glowRadius = 0f;
                glowComp.Props.glowColor = darkColor;
            }
            return glower;
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn();
            glower.Destroy();
        }


        private void AssignWindowSkyArea()
        {
            if (Rotation == Rot4.North)
            {
                skyRect = CellRectSouth(Position);
            }
            else if (Rotation == Rot4.South)
            {
                skyRect = CellRectNorth(Position);
            }
            else if (Rotation == Rot4.East)
            {
                skyRect = CellRectWest(Position);
            }
            else
            {
                skyRect = CellRectEast(Position);
            }
        }


        public override void TickRare()
        {
            base.TickRare();

            if (Spawned)
            {
                // Update skylight value base on outside obstruction
                skyLight = GetAverageGlow();
                // Check for a despawned glower
                ResetDespawnedGlower();
                // Update the glower
                UpdateGlower();
            }
        }


        private void ResetDespawnedGlower()
        {
            // If the glower managed to despawn, reset it
            if (glower == null)
            {
                glower = SpawnGlower(ViewCell);
            }
        }


        private float GetAverageGlow()
        {
            int blockedCells = 0;
            // Iterate through the cells, counting any clear cells
            foreach (IntVec3 c in skyRect)
            {
                // If this cell is blocked, count it
                if (Map.roofGrid.Roofed(c))
                {
                    blockedCells++;
                    continue;
                }
                List<Thing> thingsInCell = Map.thingGrid.ThingsListAtFast(c);
                for (int t = 0; t < thingsInCell.Count; t++)
                {
                    if (thingsInCell[t].def == ThingDefOf.Wall)
                    {
                        blockedCells++;
                        break;
                    }
                    if (thingsInCell[t].def.passability == Traversability.Impassable || thingsInCell[t].def.blockLight || thingsInCell[t].def.blockWind)
                    {
                        blockedCells++;
                        break;
                    }
                }
            }
            // Return the amount of clearance
            return ((15f - blockedCells) / 15f) * mgr.FactoredSunlight;
        }


        private void UpdateGlower()
        {
            float glow = GetAverageGlow();
            // Change the color and radius based on sky colors and clearance
            glowComp.Props.glowColor = (glow == 0f) ? darkColor : mgr.FactoredColor * (0.45f + Map.skyManager.CurSkyGlow);
            glowComp.Props.glowRadius = UnityEngine.Mathf.Lerp(1f, 7f, glow);

            // If the WindowGlow is overlit, add an overlightRadius
            // This is useful for building greenhouses
            if (glow >= 0.98f)
            {
                glowComp.Props.overlightRadius = 3.5f;
            }
            else if (glow >= 0.9f)
            {
                glowComp.Props.overlightRadius = 1.167f;
            }
            else
            {
                glowComp.Props.overlightRadius = 0f;
            }

            // Finalize the update
            Map.glowGrid.MarkGlowGridDirty(glower.Position);
        }


        public override string GetInspectString()
        {
            return "WIN_InspectWindow".Translate(GenMath.RoundedHundredth(skyLight * 100));
        }
    }
}
