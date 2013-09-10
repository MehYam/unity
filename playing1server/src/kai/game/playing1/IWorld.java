package kai.game.playing1;

import java.util.List;

public interface IWorld
{
	// world state
	public long now();
	
	public List<Actor> getMobs();
	public List<Actor> getPlayers();
	public List<Actor> getFiredAmmo();
}
