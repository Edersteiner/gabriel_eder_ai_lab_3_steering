# gabriel_eder_ai_lab_3_steering
School project showcasing steering behaviour for pathfinding agents.

Dont go snooping here unless you are my teacher, you are wasting your time.

There are two videos in the repo showing a single agent and multiple agents steering.

## Reflection

1. In your own words, what is the difference between:
   
  - A global path / waypoint list, and
  - A local steering behaviour like Seek or Arrive?

A global path or a waypoint list generally only describes a path to take through the world.
Steering is actually responsible for traversing between those paths in a natural way contrary to only linearly moving towards waypoints at a constant pace.

2. What visual difference did you notice between:
 - An agent using Seek, and
 - An agent using Arrive?

Agents using seek overshoot targets and start moving in a orbit like fashion when close to the target position.
Arrive on the other hand is similiar in terms of normal movement but introduces a braking distance leading to agents actually slowing down and stopping when reaching their target positions.

In my project, I decided to use both of these when integrating pathfinding for the agents.
I noticed that always using Arrive led to the agent stopping at each point breifly before continuing to move.
This felt unnaturual so my solution was that the agents always use seek on intermediate path points and only use arrive when moving towards the last point in the path.

3. How did Separation change the behaviour of your group?
- What happens if you set separationStrength very low? Very high?

Separation made the group spread out across a wider area.
Having separation strength or it's weight very low caused a lot of the agents to collide with each other which impacted their movement leading to small erratic micromovements.
On the other hand, having a very high separation strength and area caused agents to spread out in a visible pattern.
High separation also interferces with the pathing leading to agents getting pushed into walls, espically when many agents try to traverse a narrow area.

4. Looking ahead to your final project:
- Name at least one NPC, enemy, or unit that could use this SteeringAgent.
- How might you combine steering with your FSM or pathfinding in that
project?

This type of steering isn't exactly precise but I see it being used for aggressive NPCs (Dumb NPCs that really only need to reach their goal quickly), espically those moving in flocks, perhaps a zombie horde since those generally dont require very precise navigation across complex terrain.
Currently, im planning to make one smart and precise enemy for my final project, so this might not be fitting. But perhaps with some tweaking it could provide more precise movement. I will probably use another steering method such as simply setting velocity to the path point direction multiplied by speed, and also lerping or easing that speed by set acceleration and decelleration properties (allowing my to easily tweak movement for more precise following of paths).

Combining Steering, Pathfinding and FSM gives you a basic but robust toolset for game AI since Locomotion, Pathing and Logic will be provided. For my project I will probably use something more akin to behaviour trees or GOAP for the logic part since FSM isn't especially emergent or scalable.

But for the zombie horde example this trio is perfect.

Each zombie can have a simple FSM with simple states such as Idle, Wander, Search or chase. The pathfinding provides different paths to points determined by the states, and the steering moves along that path while also respecting the rest of the horde with the simple separation rules.



