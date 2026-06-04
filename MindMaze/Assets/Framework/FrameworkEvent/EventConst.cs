namespace Framework.FrameworkEvent
{
    /// <summary>
    /// 该类用于创建名称的常量表 (建议使用)
    /// </summary>
    public static class EventConst
    {
        public static string MonsterDead => "MonsterDead";
        
        // 路径点相关事件
        public const string WAYPOINT_REACHED = "WaypointReached";           // 到达路径点
        public const string NEXT_WAYPOINT_SET = "NextWaypointSet";         // 设置下一个路径点
        public const string TASK_COMPLETED = "TaskCompleted";               // 任务完成
        
        // 无人机相关事件
        public const string DRONE_HEALTH_CHANGED = "DroneHealthChanged";   // 无人机血量变化
        public const string DRONE_COLLISION = "DroneCollision";             // 无人机碰撞
        public const string DRONE_DESTROYED = "DroneDestroyed";             // 无人机被摧毁
        
        // UI相关事件
        public const string UI_OPEN_PANEL = "UIOpenPanel";                  // 打开面板
        public const string UI_CLOSE_PANEL = "UIClosePanel";                // 关闭面板
        public const string SCENE_LOADED = "SceneLoaded";                   // 场景加载完成
        
        // 游戏流程事件
        public const string GAME_STARTED = "GameStarted";                   // 游戏开始
        public const string GAME_PAUSED = "GamePaused";                     // 游戏暂停
        public const string GAME_RESUMED = "GameResumed";                   // 游戏恢复
        public const string GAME_OVER = "GameOver";                         // 游戏结束
        
        // 视频录制事件
        public const string RECORDING_STARTED = "RecordingStarted";         // 录制开始
        public const string RECORDING_STOPPED = "RecordingStopped";         // 录制停止
        public const string RECORDING_ERROR = "RecordingError";             // 录制错误
    }
}