using RimWorld;
using Verse;

namespace WindowMod
{

    public class CompWallAddon : ThingComp
    {

        public override void CompTickRare()
        {

            Building edifice = parent.Position.GetEdifice(parent.Map);

            if (edifice == null || edifice.def == null || (edifice.def != ThingDefOf.Wall &&
                ((edifice.Faction == null || edifice.Faction != Faction.OfPlayer) ||
                edifice.def.graphicData == null || edifice.def.graphicData.linkFlags == 0 || (LinkFlags.Wall & edifice.def.graphicData.linkFlags) == LinkFlags.None)))
            {
                parent.Destroy(DestroyMode.Deconstruct);
            }
        }
    }
}
