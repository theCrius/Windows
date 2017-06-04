using UnityEngine;
using Verse;

namespace WindowMod {
  // Tells each Window what stats its GlowComp should have based on the weather, so each one doesn't have to calculate the same stats
  public class WindowManager : MapComponent {

    private WeatherDef weatherDef;
    private float lastSunStrength;
    private float curSunStrength = 0f;
    private TickManager tickMan;

    private static WeatherDef Clear =             WeatherDef.Named("Clear");
    private static WeatherDef Fog =               WeatherDef.Named("Fog");
    private static WeatherDef Rain =              WeatherDef.Named("Rain");
    private static WeatherDef SnowGentle =        WeatherDef.Named("SnowGentle");
    private static WeatherDef FoggyRain =         WeatherDef.Named("FoggyRain");
    private static WeatherDef SnowHard =          WeatherDef.Named("SnowHard");
    private static WeatherDef DryThunderstorm =   WeatherDef.Named("DryThunderstorm");
    private static WeatherDef RainyThunderstorm = WeatherDef.Named("RainyThunderstorm");

    public float FactoredSunlight {
      get {
        if (curSunStrength == 0f) {
          GetSunlight();
        }
        return map.skyManager.CurSkyGlow * Mathf.Lerp(lastSunStrength, curSunStrength, TransitionLerpFactor);
      }
    }

    private float TransitionLerpFactor {
      get {
        return (tickMan.TicksGame % 2000) / 2000f;
      }
    }


    public WindowManager(Map map) : base(map) {
      tickMan = Find.TickManager;
    }


    public override void MapComponentTick() {
      base.MapComponentTick();
      if (tickMan.TicksGame % 250 == 0) {
        GetSunlight();
      }
      if (tickMan.TicksGame % 2000 == 0) {
        // Save the last sun strength
        lastSunStrength = curSunStrength;
        // Get the current weather
        weatherDef = map.weatherManager.curWeather;
      }
    }


    private void GetSunlight() {

      // Clear weather provides the maximum sunlight
      if (weatherDef == Clear) {
        curSunStrength = 1f;
        return;
      }
      // These weathers provide 60% sunlight
      else if (weatherDef == Fog ||
               weatherDef == Rain ||
               weatherDef == SnowGentle) {
        curSunStrength = 0.6f;
        return;
      }
      // These weathers get only 35% sunlight
      else if (weatherDef == FoggyRain ||
               weatherDef == SnowHard ||
               weatherDef == DryThunderstorm ||
               weatherDef == RainyThunderstorm) {
        curSunStrength = 0.35f;
        return;
      }
      // Default variable. Prevents issues when other mods add custom weather
      else {
        curSunStrength = 1f;
      }
    }
  }
}
