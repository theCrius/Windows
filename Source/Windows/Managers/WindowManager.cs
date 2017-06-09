using UnityEngine;
using RimWorld;
using Verse;

namespace WindowMod {
  // Tells each Window what stats its GlowComp should have based on the weather, so each one doesn't have to calculate the same stats
  public class WindowManager : MapComponent {

    private WeatherDef lastWeatherDef = Clear;
    private WeatherDef curWeatherDef = Clear;
    private float lastSunStrength;
    private float curSunStrength = 0f;
    private TickManager tickMan;
    private WeatherManager weatherMan;
    private SkyManager skyMan;

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
        return skyMan.CurSkyGlow * Mathf.Lerp(lastSunStrength, curSunStrength, TransitionLerpFactor);
      }
    }

    public ColorInt FactoredColor {
      get {
        return new ColorInt(SkyTarget.Lerp(lastWeatherDef.Worker.CurSkyTarget(map), curWeatherDef.Worker.CurSkyTarget(map), weatherMan.TransitionLerpFactor).colors.sky);
      }
    }

    private float TransitionLerpFactor {
      get {
        return (tickMan.TicksGame % 2000) / 2000f;
      }
    }


    public WindowManager(Map map) : base(map) {
      tickMan = Find.TickManager;
      weatherMan = map.weatherManager;
      skyMan = map.skyManager;
    }


    public override void MapComponentTick() {
      base.MapComponentTick();
      if (tickMan.TicksGame % 250 == 0) {
        GetSunlight();
      }
      if (tickMan.TicksGame % 2000 == 0) {
        // Save the last sun strength
        lastSunStrength = curSunStrength;
        // Save the last weather
        lastWeatherDef = weatherMan.lastWeather;
        // Get the current weather
        curWeatherDef = weatherMan.curWeather;
      }
    }


    private void GetSunlight() {

      // Clear weather provides the maximum sunlight
      if (curWeatherDef == Clear) {
        curSunStrength = 1f;
        return;
      }
      // These weathers provide 60% sunlight
      else if (curWeatherDef == Fog ||
               curWeatherDef == Rain ||
               curWeatherDef == SnowGentle) {
        curSunStrength = 0.6f;
        return;
      }
      // These weathers get only 35% sunlight
      else if (curWeatherDef == FoggyRain ||
               curWeatherDef == SnowHard ||
               curWeatherDef == DryThunderstorm ||
               curWeatherDef == RainyThunderstorm) {
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
