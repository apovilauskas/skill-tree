namespace skill_tree.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillStatus Status { get; set; } = SkillStatus.Locked;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<SkillPrerequisite> Prerequisites { get; set; } = new();
    public List<SkillLog> SkillLogs { get; set; } = new();
    public string Metric {get; set;} = string.Empty; //A label like "Matches", "Hours"
    public double Target { get; set; } = 100;

    public double Progress()
    {
        if (SkillLogs.Count == 0) return 0.0;
        if (Target <= 0) return 0.0;
        
        //v is matches/chapters/attempts/hours and T is target amount.
        double v = SkillLogs.Sum(h => h.Amount);
        double t = Target;
        
        //d1 is total days practiced, d2 is total days since beginning to unlock skill
        var practicedDays = SkillLogs.Select(s => s.Date).Distinct().OrderByDescending(d => d.Date).ToList();
        double d1 = practicedDays.Count;
        double d2 = (DateTime.UtcNow - CreatedAt).Days;
        if (d2 < 1) d2 = 1;
        
        //s is current streak
        int s = 0;
        if(d1 > 0)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday =  today.AddDays(-1);
            var mostRecentLog = practicedDays[0];
            if (today == mostRecentLog.Date || yesterday == mostRecentLog.Date)
            {
                s = 1;
                for (int i = 0; i < practicedDays.Count-1; i++)
                {
                    if (practicedDays[i] == practicedDays[i + 1].AddDays(1)) s++;
                    else break;
                } 
            }
        }
        
        //Consistency multiplier c, rewards for consistency and streak
        double c = 0.8 + 0.4 * (0.5 * d1 / d2 + 0.5 * Math.Min(1, s / 30.0));
        
        return Math.Min(100.0, v / t * c * 100.0); 
    }
    
}