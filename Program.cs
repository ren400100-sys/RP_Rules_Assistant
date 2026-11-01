using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RPRulesAssistant;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class Rule
{
    public string Number { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Keywords { get; init; } = "";
}

public sealed class MainForm : Form
{
    private readonly TextBox questionBox = new();
    private readonly Label answerLabel = new();
    private readonly Label confidenceLabel = new();
    private readonly FlowLayoutPanel matchesPanel = new();
    private readonly List<Rule> rules;

    private readonly Color Bg = Color.FromArgb(20, 16, 12);
    private readonly Color Panel = Color.FromArgb(36, 28, 19);
    private readonly Color Panel2 = Color.FromArgb(27, 21, 15);
    private readonly Color TextMain = Color.FromArgb(241, 230, 200);
    private readonly Color TextMuted = Color.FromArgb(188, 175, 135);
    private readonly Color Gold = Color.FromArgb(214, 181, 72);
    private readonly Color Border = Color.FromArgb(101, 81, 37);

    public MainForm()
    {
        rules = BuildRules();

        Text = "RP Rules Assistant";
        Width = 1120;
        Height = 820;
        MinimumSize = new Size(920, 680);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg;
        ForeColor = TextMain;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Bg
        };
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(main);

        var titlePanel = new Panel { Dock = DockStyle.Fill, Height = 72, BackColor = Bg };
        var title = new Label
        {
            AutoSize = true,
            Text = "RP Rules Assistant",
            Font = new Font("Segoe UI Semibold", 24F),
            ForeColor = Color.FromArgb(247, 231, 176),
            Location = new Point(0, 0)
        };
        var sub = new Label
        {
            AutoSize = true,
            Text = "Поиск ответа по правилам RP • Богемия 1403",
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextMuted,
            Location = new Point(2, 43)
        };
        titlePanel.Controls.Add(title);
        titlePanel.Controls.Add(sub);
        main.Controls.Add(titlePanel, 0, 0);

        var askCard = CreateCard();
        askCard.Padding = new Padding(16);
        askCard.Height = 190;

        var askLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Panel
        };
        askLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        askLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        askLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        askCard.Controls.Add(askLayout);

        askLayout.Controls.Add(new Label
        {
            Text = "Введите вопрос",
            AutoSize = true,
            ForeColor = TextMain,
            Font = new Font("Segoe UI Semibold", 10F),
            Margin = new Padding(0, 0, 0, 8)
        });

        questionBox.Multiline = true;
        questionBox.Dock = DockStyle.Fill;
        questionBox.ScrollBars = ScrollBars.Vertical;
        questionBox.BackColor = Color.FromArgb(14, 11, 8);
        questionBox.ForeColor = TextMain;
        questionBox.BorderStyle = BorderStyle.FixedSingle;
        questionBox.Font = new Font("Segoe UI", 11F);
        questionBox.Margin = new Padding(0, 0, 0, 10);
        askLayout.Controls.Add(questionBox);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Panel,
            Margin = new Padding(0)
        };
        var searchButton = MakeButton("Найти ответ", true);
        searchButton.Click += (_, _) => SearchQuestion();
        var clearButton = MakeButton("Очистить", false);
        clearButton.Click += (_, _) =>
        {
            questionBox.Clear();
            answerLabel.Text = "Введите вопрос выше.";
            confidenceLabel.Text = "";
            matchesPanel.Controls.Clear();
            questionBox.Focus();
        };
        buttonRow.Controls.Add(searchButton);
        buttonRow.Controls.Add(clearButton);
        askLayout.Controls.Add(buttonRow);
        main.Controls.Add(askCard, 0, 1);

        var answerCard = CreateCard();
        answerCard.Padding = new Padding(16);
        answerCard.Margin = new Padding(0, 14, 0, 14);
        var answerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Panel
        };
        answerLayout.Controls.Add(new Label
        {
            Text = "Ответ",
            AutoSize = true,
            ForeColor = Gold,
            Font = new Font("Segoe UI Semibold", 10F),
            Margin = new Padding(0, 0, 0, 6)
        });
        answerLabel.AutoSize = true;
        answerLabel.MaximumSize = new Size(980, 0);
        answerLabel.Text = "Введите вопрос выше. Например: «Можно ли ограбить путника без разрешения администрации?»";
        answerLabel.ForeColor = TextMain;
        answerLabel.Font = new Font("Segoe UI", 11.5F);
        answerLabel.Margin = new Padding(0, 0, 0, 6);
        answerLayout.Controls.Add(answerLabel);

        confidenceLabel.AutoSize = true;
        confidenceLabel.ForeColor = Color.FromArgb(157, 191, 137);
        confidenceLabel.Font = new Font("Segoe UI", 9F);
        answerLayout.Controls.Add(confidenceLabel);
        answerCard.Controls.Add(answerLayout);
        main.Controls.Add(answerCard, 0, 2);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Bg
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));

        var matchesCard = CreateCard();
        matchesCard.Margin = new Padding(0, 0, 10, 0);
        matchesCard.Padding = new Padding(14);
        var matchesOuter = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Panel
        };
        matchesOuter.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        matchesOuter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        matchesOuter.Controls.Add(new Label
        {
            Text = "Подходящие пункты правил",
            AutoSize = true,
            ForeColor = Color.FromArgb(217, 199, 142),
            Font = new Font("Segoe UI Semibold", 10F),
            Margin = new Padding(0, 0, 0, 10)
        });
        matchesPanel.Dock = DockStyle.Fill;
        matchesPanel.AutoScroll = true;
        matchesPanel.FlowDirection = FlowDirection.TopDown;
        matchesPanel.WrapContents = false;
        matchesPanel.BackColor = Panel;
        matchesOuter.Controls.Add(matchesPanel);
        matchesCard.Controls.Add(matchesOuter);
        body.Controls.Add(matchesCard, 0, 0);

        var examplesCard = CreateCard();
        examplesCard.Padding = new Padding(14);
        var examples = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Panel
        };
        examples.Controls.Add(new Label
        {
            Text = "Быстрые примеры",
            AutoSize = true,
            ForeColor = Color.FromArgb(217, 199, 142),
            Font = new Font("Segoe UI Semibold", 10F),
            Margin = new Padding(0, 0, 0, 10)
        });

        AddExample(examples, "Информация со стрима", "Можно ли использовать информацию со стрима, чтобы найти спрятанный предмет?");
        AddExample(examples, "Ограбление без разрешения", "Можно ли ограбить путника без разрешения администрации?");
        AddExample(examples, "Смерть и возврат в бой", "После смерти в бою можно ли сразу вернуться обратно?");
        AddExample(examples, "Перемирие между фракциями", "Нужно ли разрешение администрации на перемирие между фракциями?");
        AddExample(examples, "Крестьянин в латах", "Может ли бедный крестьянин постоянно носить дорогие латы?");
        examplesCard.Controls.Add(examples);
        body.Controls.Add(examplesCard, 1, 0);

        main.Controls.Add(body, 0, 3);

        main.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Локальная база. Интернет не используется. В спорной ситуации приоритет имеет точный текст официальных правил.",
            ForeColor = Color.FromArgb(143, 132, 106),
            Font = new Font("Segoe UI", 8.5F),
            Margin = new Padding(2, 12, 0, 0)
        }, 0, 4);

        questionBox.KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                SearchQuestion();
                e.SuppressKeyPress = true;
            }
        };

        Shown += (_, _) => questionBox.Focus();
    }

    private Panel CreateCard() =>
        new()
        {
            Dock = DockStyle.Fill,
            BackColor = Panel,
            BorderStyle = BorderStyle.FixedSingle
        };

    private Button MakeButton(string text, bool primary)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(101, 82, 31) : Color.FromArgb(55, 45, 28),
            ForeColor = TextMain,
            FlatAppearance = { BorderColor = primary ? Color.FromArgb(196, 165, 66) : Border },
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(0, 0, 10, 0),
            Cursor = Cursors.Hand
        };
    }

    private void AddExample(Control parent, string label, string question)
    {
        var b = MakeButton(label, false);
        b.Width = 280;
        b.TextAlign = ContentAlignment.MiddleLeft;
        b.Margin = new Padding(0, 0, 0, 8);
        b.Click += (_, _) =>
        {
            questionBox.Text = question;
            SearchQuestion();
        };
        parent.Controls.Add(b);
    }

    private void SearchQuestion()
    {
        var q = questionBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(q))
        {
            answerLabel.Text = "Введите вопрос.";
            confidenceLabel.Text = "";
            return;
        }

        var ranked = rules
            .Select(r => new { Rule = r, Score = ScoreRule(q, r) })
            .OrderByDescending(x => x.Score)
            .ToList();

        var best = ranked[0];
        answerLabel.Text = DirectAnswer(q, best.Rule);

        var confidence = best.Score >= 12 ? "высокая" : best.Score >= 6 ? "средняя" : "низкая";
        confidenceLabel.Text = $"Уверенность поиска: {confidence} • основной пункт: {best.Rule.Number}";

        matchesPanel.SuspendLayout();
        matchesPanel.Controls.Clear();
        foreach (var item in ranked.Take(3))
            matchesPanel.Controls.Add(CreateMatchCard(item.Rule, item.Score));
        matchesPanel.ResumeLayout();
    }

    private Control CreateMatchCard(Rule r, double score)
    {
        var p = new Panel
        {
            Width = 650,
            Height = 125,
            BackColor = Panel2,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(12)
        };

        var title = new Label
        {
            Text = $"{r.Number}. {r.Title}",
            AutoSize = true,
            ForeColor = Color.FromArgb(240, 216, 121),
            Font = new Font("Segoe UI Semibold", 10F),
            Location = new Point(10, 10)
        };
        var desc = new Label
        {
            Text = r.Description,
            AutoSize = false,
            Width = 615,
            Height = 67,
            ForeColor = Color.FromArgb(214, 205, 179),
            Font = new Font("Segoe UI", 9F),
            Location = new Point(10, 34)
        };
        var scoreLabel = new Label
        {
            Text = $"Совпадение: {score:0.0}",
            AutoSize = true,
            ForeColor = Color.FromArgb(143, 132, 106),
            Font = new Font("Segoe UI", 8F),
            Location = new Point(10, 101)
        };
        p.Controls.Add(title);
        p.Controls.Add(desc);
        p.Controls.Add(scoreLabel);
        return p;
    }

    private double ScoreRule(string q, Rule r)
    {
        var queryTokens = Tokens(q);
        var hay = Normalize($"{r.Number} {r.Title} {r.Description} {r.Keywords}");
        double score = 0;

        foreach (var t in queryTokens)
        {
            if (hay.Contains(t))
                score += t.Length >= 6 ? 4 : 2;
        }

        var nq = Normalize(q);
        if (nq.Contains("без разреш") && hay.Contains("ограб")) score += 6;
        if ((nq.Contains("стрим") || nq.Contains("трансляц")) && hay.Contains("метагейм")) score += 6;
        if (nq.Contains("10 минут") && hay.Contains("ранен")) score += 5;
        if (nq.Contains("бедн") && nq.Contains("крестьян") && hay.Contains("соответствие")) score += 5;
        if (nq.Contains("перемир") && hay.Contains("союз")) score += 5;
        if (nq.Contains("кулак") && hay.Contains("риск")) score += 5;
        return score;
    }

    private string DirectAnswer(string q, Rule best)
    {
        var s = Normalize(q);

        if ((s.Contains("ограб") || s.Contains("граб")) &&
            (s.Contains("без разреш") || s.Contains("одобр") || s.Contains("администрац")))
            return "Нет. Для ограбления требуется одобрение администрации. Основной раздел: 6 «Ограбления».";

        if ((s.Contains("стрим") || s.Contains("трансляц") || s.Contains("discord") || s.Contains("дискорд")) &&
            (s.Contains("информац") || s.Contains("узнал") || s.Contains("увидел") || s.Contains("найти")))
            return "Нет. Это MG: персонаж не получил эту информацию в рамках RP. Основной раздел: 4 «Метагейминг».";

        if ((s.Contains("без причины") || s.Contains("ради развлеч")) &&
            (s.Contains("напасть") || s.Contains("убить") || s.Contains("урон")))
            return "Нет. Нападение или убийство без достаточной RP-причины нарушает правило RDM. Основной раздел: 5.";

        if (s.Contains("перемир") || s.Contains("мирное соглаш") || s.Contains("союз"))
            return "Предварительное одобрение администрации не требуется. Союз, перемирие или мир заключаются руководством фракций по RP-логике, но администрацию нужно уведомить. Пункт 16.5.";

        if ((s.Contains("бедн") || s.Contains("крестьян")) && (s.Contains("лат") || s.Contains("доспех")))
            return "Постоянное ношение дорогих лат бедным крестьянином допустимо только при убедительном RP-обосновании, соответствующем его положению. Раздел 2.";

        if ((s.Contains("смерт") || s.Contains("умер") || s.Contains("погиб")) &&
            (s.Contains("бой") || s.Contains("лекар") || s.Contains("врач") || s.Contains("вернут")))
            return "После тяжёлого ранения минимум 10 минут нужно отыгрывать последствия. Возвращаться в тот же бой или на место смерти нельзя. Раздел 9.";

        if (s.Contains("криминальн") && s.Contains("фракц"))
            return "Организованную криминальную фракцию нужно предварительно согласовать с администрацией. Раздел 15.";

        if (s.Contains("страж") && (s.Contains("преступ") || s.Contains("глаз") || s.Contains("вид")))
            return "Нужно учитывать присутствие стражи. Если противоправное действие происходит у неё на глазах, следует сообщить администрации. Раздел 11.";

        if ((s.Contains("кулак") || s.Contains("без оруж")) &&
            (s.Contains("нескольк") || s.Contains("вооруж")))
            return "Это может считаться заведомо рискованным действием. Смерть в такой ситуации может повлечь CK. Раздел 7.";

        return $"Наиболее подходящее правило: раздел {best.Number} «{best.Title}». {best.Description}";
    }

    private static string Normalize(string s)
    {
        s = s.ToLowerInvariant().Replace('ё', 'е');
        s = Regex.Replace(s, @"[^a-zа-я0-9\.\s/-]", " ");
        return Regex.Replace(s, @"\s+", " ").Trim();
    }

    private static HashSet<string> Tokens(string s)
    {
        var stop = new HashSet<string>(new[]
        {
            "и","в","во","на","с","со","к","по","за","из","от","до","при","для","а","но","или","ли",
            "что","как","это","его","ее","их","вы","можно","нельзя","нужно","надо","если","персонаж","игрок"
        });

        var syn = new Dictionary<string, string[]>
        {
            ["грабить"] = new[] {"ограбление","грабеж"},
            ["ограбить"] = new[] {"ограбление","грабеж"},
            ["убить"] = new[] {"убийство","rdm"},
            ["напасть"] = new[] {"нападение","rdm"},
            ["стрим"] = new[] {"трансляция","mg","метагейминг"},
            ["трансляция"] = new[] {"стрим","mg","метагейминг"},
            ["дискорд"] = new[] {"discord","mg","метагейминг"},
            ["умер"] = new[] {"смерть","ранение"},
            ["погиб"] = new[] {"смерть","ранение"},
            ["латы"] = new[] {"доспех","крестьянин"},
            ["доспех"] = new[] {"латы","крестьянин"},
            ["войну"] = new[] {"война"},
            ["перемирие"] = new[] {"мир","союз"},
            ["пленного"] = new[] {"плен","пленный"},
            ["пытать"] = new[] {"пытки","жестокий"},
            ["стражник"] = new[] {"стража","npc"},
            ["стражники"] = new[] {"стража","npc"},
            ["криминальную"] = new[] {"криминальная","фракция"}
        };

        var result = new HashSet<string>();
        foreach (var w in Normalize(s).Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (stop.Contains(w)) continue;
            result.Add(w);
            if (syn.TryGetValue(w, out var arr))
                foreach (var x in arr) result.Add(x);
        }
        return result;
    }

    private static List<Rule> BuildRules() => new()
    {
        new(){Number="1",Title="Общие положения",Description="Проект действует в Богемии 1403 года. Персонаж должен соответствовать эпохе, лору, социальному положению и текущей ситуации. Запрещены сторонние программы, эксплойты и уязвимости. Игрок обязан доигрывать RP-ситуации до конца.",Keywords="общие аккаунт эксплойт ник эпоха лор"},
        new(){Number="2",Title="Соответствие роли и положению",Description="Одежда, доспехи, оружие, профессия, имущество, поведение и речь должны соответствовать роли и социальному положению. Бедный крестьянин не должен без веского RP-обоснования постоянно носить дорогие латы.",Keywords="крестьянин доспех латы оружие речь сленг положение"},
        new(){Number="3",Title="Реалистичность",Description="Игрок обязан учитывать исторический сеттинг, обстоятельства и здравый смысл. Игровая механика сама по себе не оправдывает нелогичные действия.",Keywords="реалистичность логика профессия сеттинг"},
        new(){Number="4",Title="Метагейминг (MG)",Description="Запрещено использовать информацию, которую персонаж не получил непосредственно в RP. Сведения из Discord, сторонних чатов, стримов, видеозаписей и OOC-общения использовать нельзя.",Keywords="mg метагейминг стрим трансляция discord дискорд ooc информация"},
        new(){Number="5",Title="RDM",Description="Запрещено убивать или наносить урон без достаточной ролевой причины. Нападение ради развлечения или без RP-основания является нарушением.",Keywords="rdm убийство убить нападение агрессия насилие причина"},
        new(){Number="6",Title="Ограбления",Description="Ограбление допускается только после одобрения администрации. Требуется полноценное RP-взаимодействие, /rob id и видеозапись. Обход механик ограбления запрещён.",Keywords="ограбление ограбить грабить грабеж rob имущество разрешение администрация"},
        new(){Number="7",Title="Рискованные действия",Description="Смерть вследствие заведомо рискованных действий может привести к CK. Сопротивление нескольким вооружённым противникам без соответствующего оружия или экипировки относится к рискованным действиям.",Keywords="риск рискованные ck сопротивление кулаки вооруженные"},
        new(){Number="8",Title="Character Kill (CK)",Description="CK означает полное обнуление персонажа, его знаний, имущества, средств и связей. Преднамеренный CK заранее согласовывается с администрацией.",Keywords="ck character kill обнуление персонаж"},
        new(){Number="9",Title="Тяжёлое ранение",Description="Игровая смерть по умолчанию считается тяжёлым ранением. Минимум 10 минут нужно отыгрывать плохое физическое состояние. Нельзя возвращаться в тот же бой или на место смерти. Убитого вами в бою противника можно считать погибшим.",Keywords="ранение смерть умер погиб 10 минут бой врач лекарь"},
        new(){Number="10",Title="Смерть при нарушении",Description="Если персонаж погиб из-за нарушения правил, в первую очередь рассматривается CK, затем административное наказание.",Keywords="смерть нарушение наказание ck"},
        new(){Number="11",Title="Стража и окружение",Description="Игрок обязан учитывать присутствие стражи, свидетелей и местные законы. Если противоправное действие происходит в поле зрения стражи, необходимо сообщить администрации.",Keywords="стража стражник npc свидетели законы преступление поселение"},
        new(){Number="12",Title="Метахейт",Description="Запрещены ненависть, дискриминация и предвзятое отношение к участникам сообщества, основанные на личных убеждениях игрока.",Keywords="метахейт дискриминация оскорбления идеология"},
        new(){Number="13",Title="Женские персонажи",Description="Женские персонажи являются полноценной частью RP. Навязчивое внимание, систематические провокации и создание дискомфорта без ролевых оснований являются нарушением.",Keywords="женщины женский персонаж дискриминация"},
        new(){Number="14",Title="Деструктивная деятельность",Description="Запрещены травля, целенаправленная дискредитация проекта, разжигание конфликтов и действия, направленные на дестабилизацию сообщества.",Keywords="деструктив администрация травля критика"},
        new(){Number="15",Title="Создание и развитие фракций",Description="Репутация, влияние, связи, богатство и статус должны быть обоснованы квентой или приобретены в ходе RP. Организованные криминальные фракции создаются только после согласования с администрацией. Реальные достижения существующего персонажа использовать можно.",Keywords="фракция криминальная репутация квента наемники разбойник"},
        new(){Number="16.1",Title="Панства и наёмники",Description="Наёмники участвуют в конфликтах после заключения IC-контракта с паном или его советниками. Договорённости должны быть подтверждены.",Keywords="панство наемники контракт война"},
        new(){Number="16.2",Title="Боевые действия",Description="Боевые действия делятся на малые и крупные вылазки, налёты, захваты поселений и масштабные битвы. Для каждого типа установлены лимиты и условия согласования.",Keywords="вылазка налет битва захват боевые"},
        new(){Number="16.3",Title="Объявление войны",Description="Для войны нужно не менее двух веских RP-причин, подтверждённых ситуациями и доказательствами. Война требует заявки и одобрения.",Keywords="война объявление причины заявка"},
        new(){Number="16.4",Title="Ход войны",Description="Захваты поселений и масштабные битвы проводятся только в состоянии войны. Не участвующие в войне персонажи не становятся автоматически законными целями.",Keywords="война плен переговоры захват"},
        new(){Number="16.5",Title="Союзы и перемирия",Description="Союзы, перемирия и мирные соглашения заключаются руководством фракций самостоятельно по RP-логике. Предварительное одобрение администрации не требуется, но администрацию нужно уведомить.",Keywords="перемирие мир союз капитуляция согласование"},
        new(){Number="16.6",Title="Порядок боевых действий",Description="Малая вылазка до 10 участников не требует состояния войны. Крупная вылазка до 25 участников требует одобрения. Налёт на деревню, захват поселения и масштабная битва имеют отдельные условия согласования.",Keywords="малая вылазка крупная вылазка налет деревня праймтайм"},
        new(){Number="16.8",Title="Половецкие отряды",Description="Половецкие отряды подчиняются общим правилам и лимитам численности. Разделение на группы ради обхода лимита запрещено.",Keywords="половцы отряд лимит сигизмунд"},
        new(){Number="16.10",Title="Мятежи",Description="Мятеж должен иметь веские ролевые основания и требует предварительного одобрения администрации. В заявке указываются причины, руководитель, цели, требования и участники.",Keywords="мятеж восстание руководитель заявка"},
        new(){Number="17",Title="Жестокий отыгрыш",Description="Пытки, казни и увечья допускаются только как часть RP при веской причине. Пытки требуют предварительного одобрения администрации и видеозаписи. Необратимые увечья требуют согласия игрока либо одобренного наказания.",Keywords="пытки казнь увечья жестокий"},
        new(){Number="18",Title="Пленение",Description="Пленение допускается при обоснованной RP-причине. Пленному нужно обеспечить медицинскую помощь, еду, воду и охрану. Обычный максимальный срок плена — 2 часа; при переговорах о выкупе возможны отдельные условия.",Keywords="плен пленный выкуп 2 часа охрана"},
        new(){Number="19",Title="Идентификация",Description="Имя персонажа можно узнать только если он лично представился. Узнавание по голосу ограничено правилами и требует видеодоказательства. После тяжёлого ранения голос не запоминается.",Keywords="идентификация имя голос meet представился"}
    };
}
