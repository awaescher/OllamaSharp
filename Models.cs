public class GenerateRequest
{
    public string Model { get; set; }
    public string Prompt { get; set; }
    public object Options { get; set; }
    public string System { get; set; }
    public string Template { get; set; }
}

public class CreateRequest
{
    public string Name { get; set; }
    public string Path { get; set; }
}

public class ShowRequest
{
    public string Name { get; set; }
}

public class CopyRequest
{
    public string Source { get; set; }
    public string Destination { get; set; }
}

public class PullRequest
{
    public string Name { get; set; }
    public bool Insecure { get; set; }
}

public class PushRequest
{
    public string Name { get; set; }
    public bool Insecure { get; set; }
}

public class EmbeddingsRequest
{
    public string Model { get; set; }
    public string Prompt { get; set; }
    public object Options { get; set; }
}

