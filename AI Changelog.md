# AI Changelog

week 1 and 2 were mainly research and PDR prep. 

### types of changes: 

    - **Added** for new features.
    - **Changed** for changes in existing functionality.
    - **Removed** for now removed features.

newest changes are first <br>
last updated: 2/14/2023
---

## week 5

### Added

- now can see ally players (x and z values) with rotation
- obevervaion enemy rod with players (x values)
    - not rotation bc that requires additional encoders on table

### Changed

- 52(?) tables into unity simulation

### Removed

- 

### TODO:
- [] figure out timescale & camera frame  (update rates)
- [] track enemys with camera?? (==need research==)

---

## week 4

### Added

- set *maxIdleTime* var. (already was a variable but never set with a value)
    - ~~ *maxIdleTime* var based on ball position ~~
    - *maxIdleTime* var based on ball velocity
- blue/red tag added to players in Unity
- blue/red string var. to track which team kicked the ball last in *Ball.cs*
- average rod velocity now tracted with var. (==double check==)
- Unity-chan /<3
    - vocals for goals in *GoalScored.cs*
- two different camera views (1 table with Unity-chan and all tables)
    - ==add key to switch==
- ~~placed 24 seperate tables into unity sim ~~


### Changed

- *endOfEpisode* var. set to 2500 (orginally 1000) to match *selfPlay*'s *endTime* var
- *shot reward* now a dot product of the unit vector of center of the goal and the velocity of ball
    - Higher reward for a higher ball speed (i think)
- penalty for hitting ball backward (now 4x of Goal Reward, was 2)
- penaly for getting scored on now /-4, instead of /-2

### Removed

- convex mesh ridge body on players feet (unselected *convex* option in Unity)
- zone based scoring
- draw (tie game) penalty (reward = 0)

---

## week 3

Selected computer parts and set up Unity project

### Added

- spin penality

---
