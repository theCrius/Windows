using System.Collections.Generic;

using RimWorld;
using Verse;
using System.Text;

namespace WindowMod {

  public class Building_Window : Building {

    public float WindowViewBeauty;

    private CellRect cellRect1;
    private CellRect cellRect2;
    private IntVec3 viewCell1;
    private IntVec3 viewCell2;
    private Building glower1;
    private Building glower2;
    private float skyLight1;
    private float skyLight2;
    private WindowManager mgr;


    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);

      SetFactionDirect(Faction.OfPlayerSilentFail);

      mgr = Map.GetComponent<WindowManager>();

      // Assign position info based on this window's rotation
      AssignWindowSkyAreas();

      glower1 = SpawnGlower(viewCell1);
      glower2 = SpawnGlower(viewCell2);
    }


    private static CellRect CellRectNorth(IntVec3 root) {
      return new CellRect() {
        minX = root.x - 1,
        minZ = root.z + 1,
        maxX = root.x + 1,
        maxZ = root.z + 5
      };
    }


    private static CellRect CellRectSouth(IntVec3 root) {
      return new CellRect() {
        minX = root.x - 1,
        minZ = root.z - 5,
        maxX = root.x + 1,
        maxZ = root.z - 1
      };
    }


    private static CellRect CellRectEast(IntVec3 root) {
      return new CellRect() {
        minX = root.x + 1,
        minZ = root.z - 1,
        maxX = root.x + 5,
        maxZ = root.z + 1
      };
    }


    private static CellRect CellRectWest(IntVec3 root) {
      return new CellRect() {
        minX = root.x - 5,
        minZ = root.z - 1,
        maxX = root.x - 1,
        maxZ = root.z + 1
      };
    }


    private Building SpawnGlower(IntVec3 pos) {
      // Create the glower and assign it to this faction
      Building glower = ThingMaker.MakeThing(LocalDefOf.WIN_WindowGlower) as Building;
      glower.SetFactionDirect(Faction.OfPlayerSilentFail);

      // Spawn the glower
      GenSpawn.Spawn(glower, pos, Map);

      CompGlower glowComp = new CompGlower();
      glowComp.Initialize(new CompProperties_Glower() {
        glowColor = new ColorInt(0, 0, 0, 0),
        glowRadius = 0f
      });
      glowComp.parent = glower;
      glower.AllComps.Add(glowComp);

      Map.glowGrid.RegisterGlower(glowComp);

      return glower;
    }


    public override void DeSpawn() {
      DespawnGlower(glower1);
      DespawnGlower(glower2);

      base.DeSpawn();
    }


    public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
      DeSpawn();
      base.Destroy(mode);
    }


    private void DespawnGlower(Building glower) {
      CompGlower glowComp = glower.GetComp<CompGlower>();
      Map.glowGrid.DeRegisterGlower(glowComp);
      glower.Destroy();
    }


    private void AssignWindowSkyAreas() {
      if (Rotation == Rot4.North || Rotation == Rot4.South) {
        cellRect1 = CellRectNorth(Position);
        viewCell1 = Position + IntVec3.North;
        cellRect2 = CellRectSouth(Position);
        viewCell2 = Position + IntVec3.South;
      }
      else {
        cellRect1 = CellRectEast(Position);
        viewCell1 = Position + IntVec3.East;
        cellRect2 = CellRectWest(Position);
        viewCell2 = Position + IntVec3.West;
      }
    }


    public override void TickRare() {
      base.TickRare();

      if (Spawned) {

        // If the glowers managed to despawn, reset them
        if (glower1 == null || glower2 == null) {
          if (glower1 != null && glower2 == null) {
            DespawnGlower(glower1);
          }
          else if (glower1 == null && glower2 != null){
            DespawnGlower(glower2);
          }
          glower1 = SpawnGlower(viewCell1);
          glower2 = SpawnGlower(viewCell2);
        }

        skyLight1 = GetAverageGlow(viewCell1, cellRect1);
        skyLight2 = GetAverageGlow(viewCell2, cellRect2);

        if (System.Math.Abs(skyLight1 - skyLight2) < 0.01f) {
          // no changes
        }
        else if (skyLight1 > skyLight2) {
          UpdateGlower(glower1, 0f);
          UpdateGlower(glower2, skyLight1);
        }
        else {
          UpdateGlower(glower1, skyLight2);
          UpdateGlower(glower2, 0f);
        } 
      }
    }


    private float GetAverageGlow(IntVec3 cell, CellRect rect) {
      int blockedCells = 0;
      // Iterate through the cells, counting any clear cells
      foreach (IntVec3 c in rect) {
        // If this cell is blocked, count it
        if (Map.roofGrid.Roofed(c)) {
          blockedCells++;
          continue;
        }
        List<Thing> thingsInCell = Map.thingGrid.ThingsListAtFast(c);
        for (int t = 0; t < thingsInCell.Count; t++) {
          if (thingsInCell[t].def == ThingDefOf.Wall) {
            blockedCells++;
            break;
          }
          if (thingsInCell[t].def.passability == Traversability.Impassable || thingsInCell[t].def.blockLight || thingsInCell[t].def.blockWind) {
            blockedCells++;
            break;
          }
        }
      }

      // Return the amount of clearance
      return ((15f - blockedCells) / 15f) * mgr.FactoredSunlight;
    }


    private void UpdateGlower(Building glower, float glow) {
      var radius = UnityEngine.Mathf.Lerp(1f, 7f, glow);

      // Change the color and radius based on sky colors and clearance
      var glowComp = glower.GetComp<CompGlower>();

      glowComp.Props.glowColor = mgr.FactoredColor * 1.45f;
      glowComp.Props.glowRadius = radius;

      // If the WindowGlow is overlit, add an overlightRadius
      // This is useful for building greenhouses
      if (glow >= 0.98f) {
        glowComp.Props.overlightRadius = 3.5f;
      }
      else if (glow >= 0.9f) {
        glowComp.Props.overlightRadius = 1.167f;
      }
      else {
        glowComp.Props.overlightRadius = 0f;
      }

      // Finalize the update
      Map.glowGrid.MarkGlowGridDirty(glower.Position);
    }


    public override string GetInspectString() {
      return "WIN_InspectWindow".Translate(GenMath.RoundedHundredth((skyLight1 > skyLight2) ? skyLight1 : skyLight2) * 100);
    }
  }
}
