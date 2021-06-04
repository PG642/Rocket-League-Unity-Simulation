# Physics

RL verwendet Bullet Physics Engine

## Information for video 
[link](https://www.youtube.com/watch?v=ueEmiDM94IE)
## Rocket Science YouTube
[link](https://www.youtube.com/channel/UCfKidiMlHTBRNkQZlLzUesw)
## Stat Spreadsheet
[link](https://onedrive.live.com/view.aspx?resid=F0182A0BAEBB5DFF!14583&ithint=file%2cxlsx&authkey=!ALu0cMkDZDoWOws)

## BakkesMod
BakkesMod in RocketLeague benutzen um exakte Werte abzugleichen
[link](https://www.youtube.com/watch?v=47VgRmloyQI&list=PL5hjfcWPyvtLtQcDeOZqEgcxSfPl71Wl-)


Tick rate 120 hz 8.33ms

## Car

Das Auto hats das zehnfache Gewicht des Balls (unsicher ob exakt gemeinter Wert oder übertreibung im kontext)

Kräfte unabhängig von Masse

#### Super sonic speed
* 2200uu/s
#### Max Geschwindigkeit
* 2300uu/s

Geschwindigkeiten und Positionen werden auf 2 Nachkommastellen gerundet bevor sie benutzt werden

### Boost
Boost wird als float zwischen 0 und 1 gespeichert

Darstellung als 0-100 ist nur prozentualer Anteil

Ein kleines Boostpad gibt 0.12 boost

es wird immer abgerundet

 Boost ist 13 Frames an

force/accel fixed curve

fixed mass

### Reibung
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

* Normales Fahren erzeugt 66 uu/s^2
* Boost 1058 uu/^2

* Beschleunigen bewegt das Auto in die Richtung in die es zeigt, auch wenn es in der Luft ist (sehr leicht) 
* (auch nach oben und unten wenn die spitze das autos entsprechend gerichtet ist)
* Rückwarts ist die beschleunigung leicht geringer als vorwärts


### Kräfte
 Friction Forces werden auf der Höhe des Masseschwerpunkts ausgeübt.
 
 Ball-Kraft aus Richtung des Masseschwerpunkt des Autos statt aus Berührungsnormale
 
 Hit Box war früher größer, deswegen wird der Vektor vom Masseschwerpunkt zum Ball angepasst (leicht zum schwerpunkt des Autos gedreht) und dann gekürzt
 
 Erdanziehungskraft: 650 uu/s^2
 
 Es gibt kein besonderes Verhalten für verschiedene Teile der Autohitbox (Räder ausgenommen)
 Update: doch, aber kompliziert (für Treffer auf Autodach):
  https://www.youtube.com/watch?v=CPlOh5cAizo&list=PL5hjfcWPyvtLdn_hXukGCOgt6BYpF3AbX&index=6
  
  * Fake Physik Modell (Kräfte von Masseschwerpunkt des Balls aus) und realistisches Physik Modell werden zusammen benutzt
* (Besser Video gucken für richtige erklärung mit verweis auf andere videos hier wird sehr funky)
* https://youtu.be/DRO8-784eSk?t=46


#### Stability
Rollkraft basierend auf Normale der Berührungsstelle mit dem Boden

Wenn zusätzlich Rad am Boden: Kraft nach unten -325 uu/s^2

Keine Stabilisierungskräfte wenn 3 oder mehr Räder am Boden

## Jump
* Doppelsprung dodge moves sind Impulse

* Für einen hohen Sprung kann man 200ms / 250ms (widersprüchlich) die Sprungtaste gedrückt halten. Diese Zeit zählt nicht in den Sprungtimer rein.
* Kleiner Sprung 292 uu/s
* Gedrückt halten 1458 uu/s^2 (für 0.20s -> 292 uu/s)
* Sticky Forces -325 uu/s^2 für 0.05s -> -16 uu/s


## Zweiter Sprung/Dodge
* Um einen zweiten Sprung oder eine Rolle zu machen, hat man nach dem Sprung 1.25s (1.45 bei hohem Sprung) 
* Dieser Timer ist nicht aktiviert, wenn man vom Dach der Map fällt oder anderweitig ohne den Sprung sich vom Boden löst.
* Ein zweiter Sprung gibt einem 292 uu/s Geschwindigkeit
* Doppelsprung hat fixe Geschwindigkeit unabhängig von der Dauer des Inputs
* Auf dem Boden zählt man, wenn alle 4 Reifen den Boden berühren.

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

## Wheels
* Physic preset
* Räder haben keine physikalische Breite
* Ruheposition bei Rädern (0 Schwerkraft oder in der luft) RP
* Ruheposition bei Schwerkraft auf dem Boden (still stehend oder normal fahrend): 2 uu näher an Hitbox als bei RP (federung)
* Bei Sprung: Räder bis max 12 uu unter RP
* Federung bewegt Auto erst dann, wenn Räder über RP hinaus gedrückt werden
* Räder üben keine Kraft auf Ball aus, aber der Ball übt Kraft auf die Räder aus:
* Die Radfederungen bremsen nur die Hitbox des Autos, welche dann langsamer ist wenn sie den ball berührt, was den schuss abschwächt
* Dadurch kann man vom Ball apprallen ohne dass sich dieser bewegt


## Body type 

[link](https://rocketleague.fandom.com/wiki/Body_Type)

### Hitboxen
 * Räder können rein und raus (je nachdem ob man am Boden ist oder nicht)
 * Spiel benutzt 16 bit Integer für Winkel
 * Autohitboxen sind leicht angewinkelt
 * Masseschwerpunkt ist Ankerpunkt für Sachen am Auto
 * Höhe vorne und höhe hinten bezeichnet nicht wie hoch über dem boden die hitbox anfängt, sondern in welcher höhe die hitbox endet

Die Höhe ines Autos ist  17.01 uu bei Octane

Body Type	|Length	|Width	|Height
| --- | --- | --- | ---
Octane	|118.0074000	|84.1994100	|36.1590700
Dominus	|127.9268000	|83.2799500	|31.3000000
Plank	|128.8198000	|84.6703600	|29.3944000
Breakout	|131.4924000	|80.5210000	|30.3000000
Hybrid	|127.0192000	|82.1878700	|34.1590700
Merc	|120.7200000	|76.7100000	|41.6600000


Body Type	|Standard	|Boost
| --- | --- | ---
Octane	|2.336	|2.031
Dominus	|2.336	|2.035
Plank	|2.323	|1.967
Breakout	|2.345	|2.014
Hybrid	|2.342	|2.014

#### Höhe der Hitbox

Body Type	|Ground Height
--- | --- | 
Octane:	|48.469040
Dominus:	|45.152540
Plank:	|55.860375
Breakout:	|54.858975
Hybrid:	|45.447330

#### Neigung nach vorne

Body Type	|Inclination
| --- | --- 
Octane:	|-0.55°
Dominus:	|-0.97°
Plank:	|-0.37°
Breakout:	|-0.99°
Hybrid:	|-0.55°

## Ball

* Maximale Geschwindigkeit 6000 uu/s
* Max. Spin Ball: 60 RPM = 60 Umdrehungen pro Minute
* Ball Gleitreibung bremst Ball mit 230 uu/s²
* Ball Rollt ohne zu gleiten wenn er langsamer als 565 uu/s ist
* Gleiten: Ball verliert 3% seiner geschwindigkeit pro Sekunde + Gleitreibung
* Rollen: Ball verliert 2.2% seiner geschwindigkeit pro Sekunde
* Nur wenn Ball mit langsamer als 40 uu/s UND 10 RPM für 2.5s rollt, stoppt er


* Bounces werden mit Geschwindigkeitsvektor und Oberflächennormale berechnet
* Beim Apprallen verliert der Ball 40% seiner zur Wand gerichteten Geschwindigkeit
* Der resultierende parallel zur Wand verlaufende geschwindigkeitsanteil wird durch Reibung beeinflusst


* Probleme bei Tickrate (normale stimmt nicht, da der Ball leicht im Object ist)
