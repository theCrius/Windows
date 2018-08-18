using System.Linq;
using UnityEngine;

using RimWorld;
using Verse;

namespace WindowMod
{

    public class PlaceWorker_WindowInWall : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            IntVec3 glowerSpot = loc + IntVec3.North.RotatedBy(rot);

            // Don't place out of bounds
            if (!loc.InBounds(map))
            {
                return false;
            }

            Building edifice = loc.GetEdifice(map);
            // Only allow placing on a constructed wall
            // Additional checks provided to hopefully catch any modded walls as well
            if (edifice == null || edifice.def == null || (edifice.def != ThingDefOf.Wall &&
                ((edifice.Faction == null || edifice.Faction != Faction.OfPlayer) ||
                edifice.def.graphicData == null || edifice.def.graphicData.linkFlags == 0 || (LinkFlags.Wall & edifice.def.graphicData.linkFlags) == LinkFlags.None)))
            {
                return "WIN_WindowNeedsWall".Translate();
            }

            // Ensure there is room to spawn the glower
            if (glowerSpot.Impassable(map))
            {
                return "WIN_WindowImpassable".Translate();
            }

            return true;
        }


        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            DrawCircle(center + rot.FacingCell);

            if (rot == Rot4.North)
            {
                GenDraw.DrawFieldEdges(CellRectSouth(center).Cells.ToList());
            }
            else if (rot == Rot4.South)
            {
                GenDraw.DrawFieldEdges(CellRectNorth(center).Cells.ToList());
            }
            else if (rot == Rot4.East)
            {
                GenDraw.DrawFieldEdges(CellRectWest(center).Cells.ToList());
            }
            else
            {
                GenDraw.DrawFieldEdges(CellRectEast(center).Cells.ToList());
            }
        }


        private static void DrawCircle(IntVec3 pos)
        {
            Vector3 position = pos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
        }


        private static CellRect CellRectNorth(IntVec3 center)
        {
            return new CellRect()
            {
                minX = center.x - 1,
                minZ = center.z + 1,
                maxX = center.x + 1,
                maxZ = center.z + 5
            };
        }


        private static CellRect CellRectSouth(IntVec3 center)
        {
            return new CellRect()
            {
                minX = center.x - 1,
                minZ = center.z - 5,
                maxX = center.x + 1,
                maxZ = center.z - 1
            };
        }


        private static CellRect CellRectEast(IntVec3 center)
        {
            return new CellRect()
            {
                minX = center.x + 1,
                minZ = center.z - 1,
                maxX = center.x + 5,
                maxZ = center.z + 1
            };
        }


        private static CellRect CellRectWest(IntVec3 center)
        {
            return new CellRect()
            {
                minX = center.x - 5,
                minZ = center.z - 1,
                maxX = center.x - 1,
                maxZ = center.z + 1
            };
        }
    }
}
