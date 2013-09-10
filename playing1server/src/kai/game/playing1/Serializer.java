package kai.game.playing1;

import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.entities.data.SFSObject;

public final class Serializer
{
	static public ISFSObject encode(Actor actor)
	{
		ISFSObject data = new SFSObject();
		
		data.putInt("id", actor.id);
		data.putFloat("x", actor.pos.x);
		data.putFloat("y", actor.pos.y);
		data.putFloat("dx", actor.speed.x);
		data.putFloat("dy", actor.speed.y);
		data.putFloat("r", actor.rotation);

		return data;
	}
	static public ISFSObject encode(Point point)
	{
		return null;
	}
}
