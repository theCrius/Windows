using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace WindowMod {

  internal class JoyGiver_LookOutWindow : JoyGiver {


    public override Job TryGiveJob(Pawn pawn) {
      Building glower;
      // Find the closest window glower, given the following parameters
      Predicate<Thing> validator = (Thing t) => (!t.IsForbidden(pawn)) && !t.Position.UsesOutdoorTemperature(pawn.Map) && t.Position.Standable(pawn.Map) && pawn.CanReserve(t);
      glower = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(LocalDefOf.WIN_WindowGlower), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.None, TraverseMode.ByPawn, false), 25f, validator) as Building;

      if (glower == null) {
        return null;
      }

      // Find an adjacent window
      List<IntVec3> listAdj = GenAdj.CellsAdjacentCardinal(glower).ToList();
      Building_Window window = null;

      for (int i = 0; i < 4; i++) {
        List<Thing> thingList = listAdj[i].GetThingList(pawn.Map);
        for (int t = 0; t < thingList.Count; t++) {
          if (thingList[t] != null && thingList[t] is Building_Window) {
            // If a window was found, save it
            window = thingList[t] as Building_Window;
          }
        }
      }

      if (window == null) {
        return null;
      }

      return new Job(def.jobDef, glower, window);
    }
  }
}
