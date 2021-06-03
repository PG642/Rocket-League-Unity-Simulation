# Physics

RL verwendet Bullet Physics Engine

## Information for video 
[link](https://www.youtube.com/watch?v=ueEmiDM94IE)
## Rocket Science YouTube
[link](https://www.youtube.com/channel/UCfKidiMlHTBRNkQZlLzUesw)

Fixed tick rate 120 hz 8.33ms

## Car

### Super sonic speed
2200uu/s

Max Geschiwindigkeit 
2300uu/s

### Boost
 Boost ist 13 Frames an

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

### Verhalten in der Luft
Angular Geschwindigkeit max 5.5 rad/s

#### Beschleunigung Achsen

| Achse | Geschwindigkeit
|---|---
| Yaw |9.11 rad/s^2
| Pitch |12.46 rad/s^2
| Roll |38.34 rad/s^2

Normales Fahren erzeugt 66 uu/s^2
Boost 1058 uu/^2


### Apply Force
 Friction Forces are applied in height of Center of mass
 
 Ball-Kraft aus Richtung des Masseschwerpunkt des Autos statt aus Berührungsnormale
 
 Hit Box war früher größer, deswegen wird der Vektor vom Masseschwerpunkt zum Ball angepasst (leicht zum schwerpunkt des Autos gedreht) und dann gekürzt
 Erdanziehungskraft 

650 uu/s^2

#### Stability
Rollkraft basierend auf Normale der Berührungsstelle mit dem Boden

Wenn zusätzlich Rad am Boden: Kraft nach unten -325 uu/s^2

Keine Stabilisierungskräfte wenn 3 oder mehr Räder am Boden

### Wheel position
Physic preset

## Jump
Doppelsprung dodge moves sind Impulse

Für einen hohen Sprung kann man 200ms die Sprungtaste gedrückt halten. Diese Zeit zählt nicht in den Sprungtimer rein.
Kleiner Sprung 292 uu/s
Gedrückt halten 1458 uu/s^2 (für 0.20s -> 292 uu/s)
Sticky Forces -325 uu/s^2 für 0.05s -> -16 uu/s


##Zweiter Srpung/Dodge
Um einen zweiten Sprung oder eine Rolle zu machen, hat man nach dem Sprung 1.25s (1.45 bei hohem Sprung) 
Dieser Timer ist nicht aktiviert, wenn man vom Dach der Map fällt oder anderweitig ohne den Sprung sich vom Boden löst.
Ein zweiter Sprung gibt einem 292 uu/s Geschwindigkeit

Auf dem Boden zählt man, wenn alle 4 Reifen den Boden berühren.

### Dodge
Dodge entfernt Vertikales Momentum bis auf in den ersten 0.15 s.

Danach wird die Vertikale Geschwindigkeit um 35% jeden Tick reduziert.

(Berechnet hat man eine fallende Geschwindigkeit von 15uu/s) Am besten für eine gute Erklärung das Video von Rocket Sience zu Dodges explained angucken.

Ein Dodge nach vorne oder zur Seite gibt einem 500 uu/s Geschwidigkeit. Zur Seite hängt ein Dodge von der Bewegung des Sticks ab.

Dodge zur Seite Skaliert mit Forward Geschwindigkeit. 1 + 0.9 * Geschwindigkeit/ 2300 uu/s(max) (Anteilig, wie der Stick gerichtet ist)

Rückwärts  533 uu/s * 1 + 1.5 * Geschwindigkeit/2300 uu/s

#### Dodge abbrechen
Ein Dodge kann nach den ersten 5 Ticks abgebrochen werden.
Dies ist möglich indem man den Stick in die entgegengesetze Richtung zieht. Eine Rolle kann man nicht abbrechen.
Die Stärke des Abbruchs hängt von der Stärke des Sticks ab.


## Body type 

[link](https://rocketleague.fandom.com/wiki/Body_Type)

### Hitboxen
 Räder können rein und raus (je nachdem ob man am Boden ist oder nicht)

Die Höhe ines Autos ist  17.01 uu bei Octane
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

## Ball

Maximale Geschwindigkeit 6000 uu/s

Bounces werden mit Geschwindigkeitsvektor und Oberflächennormale berechnet
Probleme bei Tickrate (normale stimmt nicht, da der Ball leicht im Object ist)
