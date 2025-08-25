namespace Crawl.ECS.System;

public class Schedule
{
    private readonly Dictionary<StageId, Stage> _namedStages = [];
    private readonly List<Stage> _stages = [];


    public Schedule AddStage(StageId stageId, Stage stage)
    {
        if (!_namedStages.TryAdd(stageId, stage))
            throw new ConflictException($"stage {stageId} already exists");

        _stages.Add(stage);

        return this;
    }

    public Stage GetStage(StageId stageId)
    {
        return _namedStages.TryGetValue(stageId, out var stage)
            ? stage
            : throw new StageNotFoundException($"stage {stageId} not found");
    }

    public void Execute(World world)
    {
        foreach (var stage in _stages) stage.Execute(world);
    }
}