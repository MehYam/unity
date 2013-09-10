package kai.game.playing1;

import kai.game.playing1.behavior.IActorBehavior;

public class Actor
{
	static int s_instances = 0; // KAI: 32-bits might not be big enough to avoid roll-over

	public final int id = ++s_instances;
	public final Point pos = new Point();
	public final Point speed = new Point();
	public float rotation = 0;
	
	public final IActorBehavior behavior;
	
	public Actor(IActorBehavior b) {behavior = b;}
}
