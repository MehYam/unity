package kai.game.playing1.behavior;

import kai.game.playing1.Actor;
import kai.game.playing1.IWorld;
import kai.game.playing1.Point;
import kai.game.playing1.Range;

public class TestRoamBehavior implements IActorBehavior
{
	static TestRoamBehavior s_instance = new TestRoamBehavior();
	public static TestRoamBehavior get()
	{
		return s_instance;
	}
	static final Range BOUNDS = new Range(-2, 2);

	// hide construction
	TestRoamBehavior() {}
	
	@Override
	public void onFrame(IWorld world, long elapsed, Actor actor)
	{
		final Point travel = new Point(actor.speed);
		travel.multiply((float)elapsed / 1000f);

		actor.pos.add(travel);
		
		float x = actor.pos.getX();
		if (x < BOUNDS.min)
		{
			actor.pos.setX(BOUNDS.min);
			actor.speed.setX(-x);
		}
		if (x > BOUNDS.max)
		{
			actor.pos.setX(BOUNDS.max);
			actor.speed.setX(-x);
		}
		
		float y = actor.pos.getY();
		if (y < BOUNDS.min)
		{
			actor.pos.setY(BOUNDS.min);
			actor.speed.setY(-y);
		}
		if (y > BOUNDS.max)
		{
			actor.pos.setY(BOUNDS.max);
			actor.speed.setY(-y);
		}

	}
}
