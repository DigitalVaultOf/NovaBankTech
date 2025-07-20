namespace Payments.Api.Utils;

public static class CustomDueDate
{
    public static DateTime BrazilianDueDate()
    {
        var now = DateTime.UtcNow;

        // Padrão brasileiro: 5 dias úteis
        var dueDate = now.AddDays(5);

        // Se cair no fim de semana, empurrar para segunda
        while (dueDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            dueDate = dueDate.AddDays(1);
        }

        Console.WriteLine($"DEBUG: DueDate calculated: {dueDate:dd/MM/yyyy} (Brazilian standard)");

        return dueDate;
    }
}