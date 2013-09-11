package kai.game.playing1;

import java.util.Date;
import java.util.LinkedList;
import java.util.List;

import kai.game.playing1.behavior.TestRoamBehavior;

public final class World implements IWorld
{
	long time = 0;

	// KAI: think about how to synchronize on these
	final List<Actor> mobs = new LinkedList<Actor>();
	final List<Actor> players = new LinkedList<Actor>();
	final List<Actor> ammo = new LinkedList<Actor>();

	public List<Actor> getMobs() { return mobs; }
	public List<Actor> getPlayers() { return players; }
	public List<Actor> getFiredAmmo() { return ammo; }

	public long now() { return time; }
	
	public World()
	{
		time = new Date().getTime();
		lastTime = time;
		
		lastTime++;
		createTestScene();
	}

	void createTestScene()
	{
		Actor one = new Actor(TestRoamBehavior.get());
		one.pos.set(1, 1);
		one.speed.set(1, 0);
		
		Actor two = new Actor(TestRoamBehavior.get());
		two.pos.set(-1, -1);
		two.speed.set(1, 0);
		
		mobs.add(one);
		mobs.add(two);
	}

	long lastTime = 0;
	public void tick()
	{
		time = new Date().getTime();
		physics(time, lastTime);
		
		lastTime = time;
	}
	
	void physics(long now, long last)
	{
		final long elapsed = now - last;
		if (elapsed > 0)
		{
			// Bounce everyone in bounds
			for (Actor actor : mobs)
			{
				actor.behavior.onFrame(this, elapsed, actor);
			}
		}

	}
}
