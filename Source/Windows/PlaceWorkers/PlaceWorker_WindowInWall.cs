using System.Linq;

using RimWorld;
using Verse;

namespace WindowMod {

  public class PlaceWorker_WindowInWall : PlaceWorker {

    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null) {
      IntVec3 adjRelativeNorth = loc + IntVec3.North.RotatedBy(rot);
      IntVec3 adjRelativeSouth = loc + IntVec3.South.RotatedBy(rot);

      // Don't place out of bounds
      if (!loc.InBounds(Map)) {
        return false;
      }

      // Ensure there is enough room to spawn a glower as well
      for (int i = 0; i < 4; i++) {
        IntVec3 c = loc + GenAdj.CardinalDirections[i];
        if (!c.InBounds(Map)) {
          return false;
        }
      }

      Building edifice = loc.GetEdifice(Map);
      // Only allow placing on a constructed wall
      // Additional checks provided to hopefully catch any modded walls as well
      if (edifice == null || edifice.def == null || (edifice.def != ThingDefOf.Wall && 
          ((edifice.Faction == null || edifice.Faction != Faction.OfPlayer) || 
          edifice.def.graphicData == null || edifice.def.graphicData.linkFlags == 0 || (LinkFlags.Wall & edifice.def.graphicData.linkFlags) == LinkFlags.None))) {
        return "WIN_WindowNeedsWall".Translate();
      }

      if (adjRelativeNorth.Impassable(Map) || adjRelativeSouth.Impassable(Map)) {
        return "WIN_WindowImpassable".Translate();
      }

      return true;
    }


    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot) {
      if (rot == Rot4.North || rot == Rot4.South) {
        GenDraw.DrawFieldEdges(CellRectNorth(center).Cells.ToList());
        GenDraw.DrawFieldEdges(CellRectSouth(center).Cells.ToList());
      }
      else {
        GenDraw.DrawFieldEdges(CellRectEast(center).Cells.ToList());
        GenDraw.DrawFieldEdges(CellRectWest(center).Cells.ToList());
      }
    }


    private static CellRect CellRectNorth(IntVec3 center) {
      return new CellRect() {
        minX = center.x - 1,
        minZ = center.z + 1,
        maxX = center.x + 1,
        maxZ = center.z + 5
      };
    }


    private static CellRect CellRectSouth(IntVec3 center) {
      return new CellRect() {
        minX = center.x - 1,
        minZ = center.z - 5,
        maxX = center.x + 1,
        maxZ = center.z - 1
      };
    }


    private static CellRect CellRectEast(IntVec3 center) {
      return new CellRect() {
        minX = center.x + 1,
        minZ = center.z - 1,
        maxX = center.x + 5,
        maxZ = center.z + 1
      };
    }


    private static CellRect CellRectWest(IntVec3 center) {
      return new CellRect() {
        minX = center.x - 5,
        minZ = center.z - 1,
        maxX = center.x - 1,
        maxZ = center.z + 1
      };
    }
  }
}
