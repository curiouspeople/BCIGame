/// <summary>
/// DFS 迷宫格子数据
/// </summary>
public class MazeCell3D
{
    public bool visited;
    public bool top = true, right = true, bottom = true, left = true;

    /// <summary> 如果该格子位于迷宫边界，打通对应的外墙（入口/出口） </summary>
    public void SetOuterWall(int x, int y, int w, int h)
    {
        if (x == 0)     left   = false;
        if (y == 0)     bottom = false;
        if (x == w - 1) right  = false;
        if (y == h - 1) top    = false;
    }
}
