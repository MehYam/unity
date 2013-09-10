package kai.game.playing1;
import java.util.List;
import java.util.concurrent.ScheduledThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

import kai.game.playing1.handlers.SpawnMeHandler;

import com.smartfoxserver.v2.entities.Room;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.ISFSObject;
import com.smartfoxserver.v2.extensions.SFSExtension;


public final class Playing1ServerExtension extends SFSExtension
{
	class WorldRunner implements Runnable
	{
		Playing1ServerExtension host;
		WorldRunner(Playing1ServerExtension host) { this.host = host; }
		public void run()
		{
			host.tick();
		}
	}

	private ScheduledThreadPoolExecutor exe;
	
	// KAI: lame, maybe nest the handlers so that we don't need this
	World _world;
	public IWorld getWorld() { return _world; }
	
	@Override
	public void init()
	{
		trace("##init()");
		
        // add handlers
		addRequestHandler("spawnMe", SpawnMeHandler.class);

        
        // create simulation
        World world = new World();
        _world = world;
        
		exe = new ScheduledThreadPoolExecutor(1);
        exe.scheduleAtFixedRate(new WorldRunner(this), 3000, 3000, TimeUnit.MILLISECONDS);
        
	}

	@Override
	public void destroy()
	{
		exe.shutdown();
		try {
			exe.awaitTermination(2000, TimeUnit.MILLISECONDS);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		super.destroy();
	}

	long lastPosUpdate = 0;
	void tick()
	{
		_world.tick();
		
		trace("##tick() " + (_world.now() - lastPosUpdate));
		if ((_world.now() - lastPosUpdate) > 200)
		{
			sendPositions();
			lastPosUpdate = _world.now();
		}
		trace("##tick done");
	}
	public void populateWorldForUser(User player)
	{
		// spawn each actor for the new user 
		for (Actor actor : _world.getMobs())
		{
			ISFSObject data = Serializer.encode(actor);
			
			//KAI: shouldn't these be batched up into an array and sent over?
			this.send("spawn", data, player);
		}
	}
	void sendPositions()
	{
		Room room = getParentRoom();
		if (room != null)
		{
			List<User> userList = room.getUserList();
	
			trace("here " + _world.getMobs().size() + ", users: " + userList.size());
			
			for (Actor actor : _world.getMobs())
			{
				ISFSObject data = Serializer.encode(actor);
				this.send("pos", data, userList, true);
			}
		}
	}
}
