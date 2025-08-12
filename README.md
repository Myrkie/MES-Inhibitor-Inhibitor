# Space Engineers MES Inhibitor Inhibitor

# What does this do?
This is a simple plugin that disables features me and my friends considered to be unfun to play with when used with certain addons inside Modular Encounters Systems, it's a server side only [Torch plugin](https://torchapi.com/)

### Torch Server plugin
1. Download the latest ZIP from [Releases](https://github.com/Myrkie/MES-Inhibitor-Inhibitor/releases).
2. Place the zip in the torch-server/Plugins folder
3. add the plugins `17f44521-b77a-4e85-810f-ee73311cf692` GUID to your `torch.cfg` and then start torch. no settings exist everything is applied at session start.
```xml
  <Plugins>
    <guid>17f44521-b77a-4e85-810f-ee73311cf692</guid>
  </Plugins>
```

## Disabled Features

Inhibitors:
1. Player Inhibitors
2. Drill Inhibitors
3. JumpDrive Inhibitors
4. NanoBot Inhibitors
5. JetPack Inhibitors
6. Energy Drain Inhibitors


Encounter Actions:
1. GenerateExplosion (allows drones to generate explosions on action, usually used for cheap drones that explode on grind without any way to diffuse)
2. DamageToolAttacker (allows drones to apply damage to an attacker, mostly used for by cheap drones that apply unsurvivable levels of damage on grind 5000~)