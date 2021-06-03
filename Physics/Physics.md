# Physics

RL verwendet Bullet Physics Engine

## Information for video 
[link](https://www.youtube.com/watch?v=ueEmiDM94IE)
## Rocket Science YouTube
[link](https://www.youtube.com/channel/UCfKidiMlHTBRNkQZlLzUesw)

Fixed tick rate 120 hz 8.33ms

## Car

force/accel fixed curve

fixed mass

Kräfte unabhängig von Masse

Keine Reibung in Längsrichtung

Reibung in Querrichtung vereinfacht:

Ratio = SideSpeed/ (SideSpeed + ForwardSpeed)

SlideFriction = Curve(Ratio)

Curve(1)=0.2

Curve(0)=1

GroundFriction = Curve2(GroundNormal.Z)

Friction = SlideFriction * GroundFriction

Impulse Contraint * Friction

### Apply Force
 Friction Forces are applied in height of Center of mass
 
 Ball-Kraft aus Richtung des Masseschwerpunkt des Autos statt aus Berührungsnormale
 
 Hit Box war früher größer, deswegen wird der Vektor vom Masseschwerpunkt zum Ball angepasst (leicht zum schwerpunkt des Autos gedreht) und dann gekürzt

#### Stability
Rollkraft basierend auf Normale der Berührungsstelle mit dem Boden

Wenn zusätzlich Rad am Boden: Kraft nach unten

Keine Stabilisierungskräfte wenn 3 oder mehr Räder am Boden

### Wheel position
Physic preset

## Jump
Doppelsprung dodge moves sind Impulse

## Body type 

[link](https://rocketleague.fandom.com/wiki/Body_Type)

Hitboxen
Body Type	|Length	|Width	|Height
--- | --- | --- | ---
Octane	|118.0074000	|84.1994100	|36.1590700
Dominus	|127.9268000	|83.2799500	|31.3000000
Plank	|128.8198000	|84.6703600	|29.3944000
Breakout	|131.4924000	|80.5210000	|30.3000000
Hybrid	|127.0192000	|82.1878700	|34.1590700
Merc	|120.7200000	|76.7100000	|41.6600000


Body Type	|Standard	|Boost
 --- | --- | ---
Octane	|2.336	|2.031
Dominus	|2.336	|2.035
Plank	|2.323	|1.967
Breakout	|2.345	|2.014
Hybrid	|2.342	|2.014

Höhe der Hitbox
Body Type	|Ground Height
--- | --- | 
Octane:	|48.469040
Dominus:	|45.152540
Plank:	|55.860375
Breakout:	|54.858975
Hybrid:	|45.447330

Neigung nach vorne
Body Type	|Inclination
--- | --- |
Octane:	|-0.55°
Dominus:	|-0.97°
Plank:	|-0.37°
Breakout:	|-0.99°
Hybrid:	|-0.55°
