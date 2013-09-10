package kai.game.playing1;

public final class Point
{
	//KAI: consider a readonly object instead, no getters...
	float x;
	float y;
	
	public Point() { this.x = 0; this.y = 0; }
	public Point(float x, float y) { this.x = x; this.y = y; }
	public Point(Point p) { this.x = p.x; this.y = p.y; }

	public float getX() { return this.x; }
	public float getY() { return this.y; }
	public void set(float x, float y) { this.x = x; this.y = y;}
	public void setX(float x) { this.x = x; }
	public void setY(float y) { this.y = y; }
	
	public void add(float c)
	{
		x += c;
		y += c;
	}
	public void add(Point point)
	{
		x += point.x;
		y += point.y;
	}
	public void multiply(float c)
	{
		x *= c;
		y *= c;
	}
	public void multiply(Point point)
	{
		x *= point.x;
		y *= point.y;
	}
}
