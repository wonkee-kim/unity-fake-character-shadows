#ifndef FAKE_SHADOWS_INCLUDED
#define FAKE_SHADOWS_INCLUDED

half4 _FakeShadowsLights[4];
const half epsilon = 0.0001;

// half4 player(pos.xyz, radius)
half CalcFakeShadowPerLight(half4 light, half3 playerPos, half playerRad, half3 posToPlayer, half3 posWS)
{
    // Calc dot
    half3 playerToLight = normalize(light.xyz - playerPos);
    half d = dot(posToPlayer, playerToLight);
    float r = 1 - playerRad;
    d = saturate((d - r) / (1 - r)); // remap range: r~1 -> 0~1

    // Attenuation
    half distLightToPos = distance(posWS, light.xyz);
    half atten = 1 - saturate(distLightToPos / (light.w + epsilon)); // Apply light radius
    atten = atten * atten; // Inverse Square Law

    // Adjust attenuation and reverse
    return 1 - saturate(d * atten);
}

// shadowArgs: (x)radius, (y)strength
void FakeShadows_float(in half3 playerPosition, in half4 shadowArgs, in half3 positionWS, out half shadows)
{
    shadows = 1;
    #if !SHADERGRAPH_PREVIEW
        half3 posToPlayer = normalize(playerPosition - positionWS);
        shadows *= CalcFakeShadowPerLight(_FakeShadowsLights[0], playerPosition, shadowArgs[0], posToPlayer, positionWS);
        shadows *= CalcFakeShadowPerLight(_FakeShadowsLights[1], playerPosition, shadowArgs[0], posToPlayer, positionWS);
        shadows *= CalcFakeShadowPerLight(_FakeShadowsLights[2], playerPosition, shadowArgs[0], posToPlayer, positionWS);
        shadows *= CalcFakeShadowPerLight(_FakeShadowsLights[3], playerPosition, shadowArgs[0], posToPlayer, positionWS);
        shadows = shadows * shadows * shadows; // enhance shadows
        shadows = saturate(shadows + (1 - shadowArgs[1]));
    #endif
}

#endif