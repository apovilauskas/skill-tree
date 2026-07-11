using System.Runtime.InteropServices.JavaScript;
using skill_tree.Entities;

namespace skill_tree.Service;

public class SkillTreeService
{
    public double Progress(Skill skill)
    {
        if (skill.SkillLogs.Count == 0) return 0.0;
        if (skill.Target <= 0) return 0.0;
        
        //if skill quantifiable, v is matches/chapters/attemps and T is 100 or amount needed. If fluid, v is hours done and t is target hours.
        double v = skill.SkillLogs.Sum(h => h.Amount);
        double t = skill.Target;
        
        //d1 is total days practiced, d2 is total days since beginning to unlock skill
        var practicedDays = skill.SkillLogs.Select(s => s.Date).Distinct().OrderByDescending(d => d.Date).ToList();
        double d1 = practicedDays.Count;
        double d2 = (DateTime.UtcNow - skill.CreatedAt).Days;
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
                    if (practicedDays[i] == practicedDays[i + 1]) continue;
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