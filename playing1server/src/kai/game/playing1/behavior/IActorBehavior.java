package kai.game.playing1.behavior;

import kai.game.playing1.Actor;
import kai.game.playing1.IWorld;

public interface IActorBehavior
{
	public void onFrame(IWorld world, long elapsed, Actor actor);
}
