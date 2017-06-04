using Verse;

namespace WindowMod {

  public class CompWallAddon : ThingComp {

    public override void CompTickRare() {

      Building wall = parent.Position.GetEdifice(parent.Map);

      if (wall == null) {
        parent.Destroy(DestroyMode.KillFinalize);
      }
    }
  }
}
