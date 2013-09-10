package kai.game.playing1.handlers;

import kai.game.playing1.Playing1ServerExtension;

import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.extensions.BaseClientRequestHandler;

public class SpawnMeHandler extends BaseClientRequestHandler
{
	@Override
	public void handleClientRequest(User player, ISFSObject arg1)
	{
		Playing1ServerExtension ext = (Playing1ServerExtension) getParentExtension();
		ext.populateWorldForUser(player);
	}
}
