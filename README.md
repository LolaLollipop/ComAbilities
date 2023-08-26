# ComAbilities
[![ComAbilities](https://github.com/Ruemena/ComAbilities/actions/workflows/action.yml/badge.svg)](https://github.com/Ruemena/ComAbilities/actions/workflows/action.yml)

#### ! Exiled plugin only (for now) !
#### PRs are VERY welcome (especially translations/localizations)

ComAbilities is a large expansion for SCP-079, adding new abilities, balance changes, and other features and quirks designed to make him both more enjoyable and more fun to play against. it's highly configurable and modular, allowing for you to pick and choose what features you would like.
#### Planned features
- [ ] flash cannon (shoot flash grenades out of the camera)
- [ ] shooting cameras
- [ ] generators causing weird effects in the facility
- [ ] NWAPI port (eventually)
## Exhaustive list of features
#### Table of Contents
- [Localizer](#localizer)
- [Access Level Permissions](#access-level-permissions)
- [Abilities](#abilities)
  - [Reality Scrambler](#reality-scrambler)
  - [Player Tracker](#player-tracker)
### Localizer
ComAbilities comes prepackaged with a localizer, allowing for the text within the plugin to be modified easily. language files are generated when the plugin is first run (default localizations will be used), in the EXILED configs folder. the current language file in use can be configured by changing the `Localization` config - for example, `English` will set the current localization file to be `English.yml`. the .yml files inside the folder can be modified freely.

### Access Level Permissions
this allows for you to give different keycard permissions to 079's levels. this makes it so that 079 can't open/close/lock these doors without the required level. for example, you can restrict science tier 2 to level 3, making it so he can't open up 096's chamber. candy room, etc without that level.

### Anti "Decontamination Locking"
this allows for you to lock all doors open when the countdown to lcz decontamination begins, making it so that 079 can't lock people out of escaping

### Abilities

#### Reality Scrambler
while active, the Reality Scrambler regenerates the Hume Shield of all SCPs, regardless of whether or not they are taking damage. however, it drains Aux Power and prevents regeneration of it. 
<details closed>
<summary>Lore</summary>
<br>
Site-02 features █ Scranton Reality Anchors, powerful devices that can nullify the abilities of reality-benders. However, in order to facilitate testing, these can be remotely disabled. Doing so greatly increases the reality-bending powers of the various anomalies with the site, so authorization from the Facility Manager is required.
</details>

#### Player Tracker
this ability allows you to tag a player by pinging them (for an aux cost), and then teleport to them later. acts as a sort of alternative to the breach scanner
<details closed>
<summary>Lore</summary>
<br>
As part of an effort to combat the increasing number of breaches by SCP-106, SCP-173, and SCP-████, a network of sensors, light detectors, and other devices was installed within the facility to act as a support system to the Breach Scanner. Known as the PLAYER Tracker, this system allows for real-time monitoring and tracking of anomalies, although it has been utilized against hostile GOI forces and rogue personnel.
</details>
