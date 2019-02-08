using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class AIPlayForm : Form
    {
        private const int rectSize = 33; // 오목판의 셀 크기
        private const int edgeCount = 15; // 오목판의 선 개수

        private enum Horse { none = 0, BLACK, WHITE };
        private Horse[,] board = new Horse[edgeCount, edgeCount];
        private Horse nowPlayer = Horse.BLACK;
        private Horse aiPlayer, userPlayer;

        private int targetX, targetY;
        private int limit = 1;
        private bool playing = false;

        private bool judge() // 승리 판정 함수
        {
            for (int i = 0; i < edgeCount - 4; i++) // 가로
                for (int j = 0; j < edgeCount; j++)
                    if (board[i, j] == nowPlayer && board[i + 1, j] == nowPlayer && board[i + 2, j] == nowPlayer &&
                        board[i + 3, j] == nowPlayer && board[i + 4, j] == nowPlayer)
                        return true;
            for (int i = 0; i < edgeCount; i++) // 세로
                for (int j = 4; j < edgeCount; j++)
                    if (board[i, j] == nowPlayer && board[i, j - 1] == nowPlayer && board[i, j - 2] == nowPlayer &&
                        board[i, j - 3] == nowPlayer && board[i, j - 4] == nowPlayer)
                        return true;
            for (int i = 0; i < edgeCount - 4; i++) // Y = X 직선
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == nowPlayer && board[i + 1, j + 1] == nowPlayer && board[i + 2, j + 2] == nowPlayer &&
                        board[i + 3, j + 3] == nowPlayer && board[i + 4, j + 4] == nowPlayer)
                        return true;
            for (int i = 4; i < edgeCount; i++) // Y = -X 직선
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == nowPlayer && board[i - 1, j + 1] == nowPlayer && board[i - 2, j + 2] == nowPlayer &&
                        board[i - 3, j + 3] == nowPlayer && board[i - 4, j + 4] == nowPlayer)
                        return true;
            return false;
        }

        private void refresh()
        {
            // 전체 Board 정보를 초기화합니다.
            for (int i = 0; i < edgeCount; i++)
                for (int j = 0; j < edgeCount; j++)
                    board[i, j] = Horse.none;
            this.boardPicture.Refresh();
            // AI와 사용자 중에서 먼저 공격할 사람을 구합니다.
            int rand = new Random().Next(1, 3);
            if (rand == 1) aiPlayer = Horse.BLACK;
            else aiPlayer = Horse.WHITE;
            userPlayer = ((aiPlayer == Horse.BLACK) ? Horse.WHITE : Horse.BLACK);
            nowPlayer = Horse.BLACK;
            if (nowPlayer == userPlayer) status.Text = "당신의 차례입니다.";
            else status.Text = "게임이 시작되었습니다.";
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                refresh();
                playing = true;
                playButton.Text = "재시작";
            }
            else
            {
                refresh();
            }
            if (nowPlayer == aiPlayer)
            {
                ai_Attack();
            }
        }

        public AIPlayForm()
        {
            InitializeComponent();
        }

        private void boardPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            Color lineColor = Color.Black; // 오목판의 선 색깔
            Pen p = new Pen(lineColor, 2);
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2); // 좌측
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize / 2); // 상측
            gp.DrawLine(p, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // 하측
            gp.DrawLine(p, rectSize * edgeCount - rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // 우측
            p = new Pen(lineColor, 1);
            // 대각선 방향으로 이동하면서 십자가 모양의 선 그리기
            for (int i = rectSize + rectSize / 2; i < rectSize * edgeCount - rectSize / 2; i += rectSize)
            {
                gp.DrawLine(p, rectSize / 2, i, rectSize * edgeCount - rectSize / 2, i);
                gp.DrawLine(p, i, rectSize / 2, i, rectSize * edgeCount - rectSize / 2);
            }
        }

        private void ai_Attack()
        {
            AlphaBetaPruning(0, -1000000, 1000000);
            board[targetX, targetY] = aiPlayer;
            Graphics g = this.boardPicture.CreateGraphics();
            if (nowPlayer == Horse.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, targetX * rectSize, targetY * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, targetX * rectSize, targetY * rectSize, rectSize, rectSize);
            }
            if (judge())
            {
                if (nowPlayer == aiPlayer) status.Text = "AI 플레이어의 승리입니다.";
                else status.Text = "당신의 승리입니다.";
                playing = false;
                playButton.Text = "게임시작";
            }
            else
            {
                nowPlayer = ((nowPlayer == Horse.BLACK) ? Horse.WHITE : Horse.BLACK);
            }
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (nowPlayer == aiPlayer) return;
            if (!playing)
            {
                MessageBox.Show("게임을 실행해주세요.");
                return;
            }
            Graphics g = this.boardPicture.CreateGraphics();
            int x = e.X / rectSize;
            int y = e.Y / rectSize;
            if (x < 0 || y < 0 || x >= edgeCount || y >= edgeCount)
            {
                MessageBox.Show("테두리를 벗어날 수 없습니다.");
                return;
            }
            if (board[x, y] != Horse.none) return;
            board[x, y] = nowPlayer;
            if (nowPlayer == Horse.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            if (judge())
            {
                if (nowPlayer == aiPlayer) status.Text = "AI 플레이어의 승리입니다.";
                else status.Text = "당신의 승리입니다.";
                playing = false;
                playButton.Text = "게임시작";
            }
            else
            {
                nowPlayer = ((nowPlayer == Horse.BLACK) ? Horse.WHITE : Horse.BLACK);
                ai_Attack();
            }
        }

        /*------------- 5목이 만들어질 수 있는 모든 20가지 경우의 수  -------------*/

        public bool make5mok1(int x, int y)
        {
            try
            {
                for (int i = y; i < y + 5; i++)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok2(int x, int y)
        {
            try
            {
                for (int i = x, j = y; i < x + 5; i++, j--)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok3(int x, int y)
        {
            try
            {
                for (int i = x; i < x + 5; i++)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok4(int x, int y)
        {
            try
            {
                for (int i = x, j = y; i < x + 5; i++, j++)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok5(int x, int y)
        {
            try
            {
                for (int i = y; i > y - 5; i--)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok6(int x, int y)
        {
            try
            {
                for (int i = x, j = y; i > x - 5; i--, j++)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok7(int x, int y)
        {
            try
            {
                for (int i = x; i > x - 5; i--)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok8(int x, int y)
        {
            try
            {
                for (int i = x, j = y; i > x - 5; i--, j--)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok9(int x, int y)
        {
            try
            {
                for (int i = y - 1; i < y + 4; i++)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok10(int x, int y)
        {
            try
            {
                for (int i = x - 1, j = y + 1; i < x + 4; i++, j--)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok11(int x, int y)
        {
            try
            {
                for (int i = x - 1; i < x + 4; i++)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok12(int x, int y)
        {
            try
            {
                for (int i = x - 1, j = y - 1; i < x + 4; i++, j++)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok13(int x, int y)
        {
            try
            {
                for (int i = y + 1; i > y - 4; i--)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok14(int x, int y)
        {
            try
            {
                for (int i = x + 1, j = y - 1; i > x - 4; i--, j++)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok15(int x, int y)
        {
            try
            {
                for (int i = x + 1; i > x - 4; i--)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok16(int x, int y)
        {
            try
            {
                for (int i = x + 1, j = y + 1; i > x - 4; i--, j--)
                {
                    if (board[i, j] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok17(int x, int y)
        {
            try
            {
                for (int i = y - 2; i < y + 3; i++)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok18(int x, int y)
        {
            try
            {
                for (int i = x - 2; i < x + 3; i++)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok19(int x, int y)
        {
            try
            {
                for (int i = y + 2; i > y - 3; i--)
                {
                    if (board[x, i] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        public bool make5mok20(int x, int y)
        {
            try
            {
                for (int i = x + 2; i > x - 3; i--)
                {
                    if (board[i, y] != board[x, y]) return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            return true;
        }

        /*------------- 3목이 만들어질 수 있는 경우의 수 판별  -------------*/

        /*
         * 기본적으로 각 방향으로 3목이 생성되면 count에 2를 설정합니다.
         * 이후에 한 쪽이라도 막혀있으면 count를 1씩 뺍니다.
         * 즉, 열린 3은 두 곳 모두 열려있으므로 2를 반환합니다.
         * 하나 닫힌 3은 1을 반환하며 닫힌 3은 0을 반환하게 됩니다.
         * 3목에 해당하지 않는 경우 -1을 반환합니다.
         * 
         */

        public int make3mok1(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x, y + 1] && board[x, y] == board[x, y + 2])
                {
                    count = 2;
                    // 4목인 경우
                    if (y < edgeCount - 3 && board[x, y] == board[x, y + 3]) return -1;
                    if (y > 0 && board[x, y] == board[x, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (y == edgeCount - 3 || board[x, y + 3] != 0) count--;
                    if (y == 0 || board[x, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok2(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y + 1] == 0 && board[x, y] == board[x, y + 2] && board[x, y] == board[x, y + 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (y < edgeCount - 4 && board[x, y] == board[x, y + 4]) return -1;
                    if (y > 0 && board[x, y] == board[x, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (y == edgeCount - 4 || board[x, y + 4] != 0) count--;
                    if (y == 0 || board[x, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok3(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y + 2] == 0 && board[x, y] == board[x, y + 1] && board[x, y] == board[x, y + 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (y < edgeCount - 4 && board[x, y] == board[x, y + 4]) return -1;
                    if (y > 0 && board[x, y] == board[x, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (y == edgeCount - 4 || board[x, y + 4] != 0) count--;
                    if (y == 0 || board[x, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok4(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y + 1] && board[x, y] == board[x - 2, y + 2])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 2 && y < edgeCount - 3 && board[x, y] == board[x - 3, y + 3]) return -1;
                    if (x < edgeCount - 1 && y > 0 && board[x, y] == board[x + 1, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 2 || y == edgeCount - 3 || board[x - 3, y + 3] != 0) count--;
                    if (x == edgeCount - 1 || y == 0 || board[x + 1, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok5(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y + 1] == 0 && board[x, y] == board[x - 2, y + 2] && board[x, y] == board[x - 3, y + 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 3 && y < edgeCount - 4 && board[x, y] == board[x - 4, y + 4]) return -1;
                    if (x < edgeCount - 1 && y > 0 && board[x, y] == board[x + 1, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 3 || y == edgeCount - 4 || board[x - 4, y + 4] != 0) count--;
                    if (x == edgeCount - 1 || y == 0 || board[x + 1, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok6(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y + 2] == 0 && board[x, y] == board[x - 1, y + 1] && board[x, y] == board[x - 3, y + 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 3 && y < edgeCount - 4 && board[x, y] == board[x - 4, y + 4]) return -1;
                    if (x < edgeCount - 1 && y > 0 && board[x, y] == board[x + 1, y - 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 3 || y == edgeCount - 4 || board[x - 4, y + 4] != 0) count--;
                    if (x == edgeCount - 1 || y == 0 || board[x + 1, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok7(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y] && board[x - 2, y] == board[x, y])
                {
                    count = 2;
                    // 4목인 경우
                    if (x < edgeCount - 1 && board[x, y] == board[x + 1, y]) return -1;
                    if (x > 2 && board[x, y] == board[x - 3, y]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == edgeCount - 1 || board[x + 1, y] != 0) count--;
                    if (x == 2 || board[x - 3, y] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok8(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y] == 0 && board[x, y] == board[x - 2, y] && board[x - 3, y] == board[x, y])
                {
                    count = 2;
                    // 4목인 경우
                    if (x < edgeCount - 1 && board[x, y] == board[x + 1, y]) return -1;
                    if (x > 3 && board[x, y] == board[x - 4, y]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == edgeCount - 1 || board[x + 1, y] != 0) count--;
                    if (x == 3 || board[x - 4, y] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok9(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y] == 0 && board[x, y] == board[x - 1, y] && board[x - 3, y] == board[x, y])
                {
                    count = 2;
                    // 4목인 경우
                    if (x < edgeCount - 1 && board[x, y] == board[x + 1, y]) return -1;
                    if (x > 3 && board[x, y] == board[x - 4, y]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == edgeCount - 1 || board[x + 1, y] != 0) count--;
                    if (x == 3 || board[x - 4, y] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok10(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y - 1] && board[x, y] == board[x - 2, y - 2])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 2 && y > 2 && board[x, y] == board[x - 3, y - 3]) return -1;
                    if (x < edgeCount - 1 && y < edgeCount - 1 && board[x, y] == board[x + 1, y + 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 2 || y == 2 || board[x - 3, y - 3] != 0) count--;
                    if (x == edgeCount - 1 || y == edgeCount - 1 || board[x + 1, y + 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok11(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y - 1] == 0 && board[x, y] == board[x - 2, y - 2] && board[x, y] == board[x - 3, y - 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 3 && y > 3 && board[x, y] == board[x - 4, y - 4]) return -1;
                    if (x < edgeCount - 1 && y < edgeCount - 1 && board[x, y] == board[x + 1, y + 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 3 || y == 3 || board[x - 4, y - 4] != 0) count--;
                    if (x == edgeCount - 1 || y == edgeCount - 1 || board[x + 1, y + 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make3mok12(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y - 2] == 0 && board[x, y] == board[x - 1, y - 1] && board[x, y] == board[x - 3, y - 3])
                {
                    count = 2;
                    // 4목인 경우
                    if (x > 3 && y > 3 && board[x, y] == board[x - 4, y - 4]) return -1;
                    if (x < edgeCount - 1 && y < edgeCount - 1 && board[x, y] == board[x + 1, y + 1]) return -1;
                    // 4목이 아닌 경우 닫혔는지 확인
                    if (x == 3 || y == 3 || board[x - 4, y - 4] != 0) count--;
                    if (x == edgeCount - 1 || y == edgeCount - 1 || board[x + 1, y + 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        /*------------- 4목이 만들어질 수 있는 경우의 수 판별  -------------*/

        /*
         * 기본적으로 각 방향으로 4목이 생성되면 count에 2를 설정합니다.
         * 이후에 한 쪽이라도 막혀있으면 count를 1씩 뺍니다.
         * 즉, 열린 4은 두 곳 모두 열려있으므로 2를 반환합니다.
         * 하나 닫힌 4은 1을 반환하며 닫힌 4은 0을 반환하게 됩니다.
         * 4목에 해당하지 않는 경우 -1을 반환합니다.
         * 
         */

        public int make4mok1(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x, y + 1] && board[x, y] == board[x, y + 2] && board[x, y] == board[x, y + 3])
                {
                    count = 2;
                    // 4목 주변으로 닫혔는지 확인
                    if (y == edgeCount - 4 || board[x, y + 4] != 0) count--;
                    if (y == 0 || board[x, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok2(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y + 1] == 0 && board[x, y] == board[x, y + 2] && board[x, y] == board[x, y + 3] && board[x, y] == board[x, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok3(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y + 2] == 0 && board[x, y] == board[x, y + 1] && board[x, y] == board[x, y + 3] && board[x, y] == board[x, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok4(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y + 3] == 0 && board[x, y] == board[x, y + 1] && board[x, y] == board[x, y + 2] && board[x, y] == board[x, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok5(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y + 1] && board[x, y] == board[x - 2, y + 2] && board[x, y] == board[x - 3, y + 3])
                {
                    count = 2;
                    // 4목 주변으로 닫혔는지 확인
                    if (x == edgeCount - 1 || y == 0 || board[x + 1, y - 1] != 0) count--;
                    if (x == 3 || y == edgeCount - 4 || board[x - 4, y + 4] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok6(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y + 1] == 0 && board[x, y] == board[x - 2, y + 2] && board[x, y] == board[x - 3, y + 3] && board[x, y] == board[x - 4, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok7(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y + 2] == 0 && board[x, y] == board[x - 1, y + 1] && board[x, y] == board[x - 3, y + 3] && board[x, y] == board[x - 4, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok8(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 3, y + 3] == 0 && board[x, y] == board[x - 1, y + 1] && board[x, y] == board[x - 2, y + 2] && board[x, y] == board[x - 4, y + 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok9(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y] && board[x, y] == board[x - 2, y] && board[x - 3, y] == board[x, y])
                {
                    count = 2;
                    // 4목 주변으로 닫혔는지 확인
                    if (x == edgeCount - 1 || board[x + 1, y] != 0) count--;
                    if (x == 3 || board[x - 4, y] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok10(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y] == 0 && board[x, y] == board[x - 2, y] && board[x, y] == board[x - 3, y] && board[x - 4, y] == board[x, y]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok11(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y] == 0 && board[x, y] == board[x - 1, y] && board[x, y] == board[x - 3, y] && board[x - 4, y] == board[x, y]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok12(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 3, y] == 0 && board[x, y] == board[x - 2, y] && board[x, y] == board[x - 1, y] && board[x - 4, y] == board[x, y]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok13(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y - 1] && board[x, y] == board[x - 2, y - 2] && board[x, y] == board[x - 3, y - 3])
                {
                    count = 2;
                    // 4목 주변으로 닫혔는지 확인
                    if (x == edgeCount - 1 || y == edgeCount - 1 || board[x + 1, y + 1] != 0) count--;
                    if (x == 3 || y == 3 || board[x - 4, y - 4] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok14(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 1, y - 1] == 0 && board[x, y] == board[x - 2, y - 2] && board[x, y] == board[x - 3, y - 3] && board[x, y] == board[x - 4, y - 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok15(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 2, y - 2] == 0 && board[x, y] == board[x - 1, y - 1] && board[x, y] == board[x - 3, y - 3] && board[x, y] == board[x - 4, y - 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make4mok16(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x - 3, y - 3] == 0 && board[x, y] == board[x - 1, y - 1] && board[x, y] == board[x - 2, y - 2] && board[x, y] == board[x - 4, y - 4]) count = 1;
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        /*------------- 2목이 만들어질 수 있는 경우의 수 판별  -------------*/

        /*
         * 기본적으로 각 방향으로 2목이 생성되면 count에 2를 설정합니다.
         * 이후에 한 쪽이라도 막혀있으면 count를 1씩 뺍니다.
         * 즉, 열린 2은 두 곳 모두 열려있으므로 2를 반환합니다.
         * 하나 닫힌 2은 1을 반환하며 닫힌 3은 0을 반환하게 됩니다.
         * 
         */

        public int make2mok1(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x, y + 1])
                {
                    count = 2;
                    // 3목인 경우
                    if (y < edgeCount - 2 && board[x, y] == board[x, y + 2]) return -1;
                    if (y > 0 && board[x, y] == board[x, y - 1]) return -1;
                    // 3목이 아닌 경우 닫혔는지 확인
                    if (y == edgeCount - 2 || board[x, y + 2] != 0) count--;
                    if (y == 0 || board[x, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }


        public int make2mok2(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y + 1])
                {
                    count = 2;
                    // 3목인 경우
                    if (x > 1 && y < edgeCount - 2 && board[x, y] == board[x - 2, y + 2]) return -1;
                    if (x < edgeCount - 1 && y > 0 && board[x, y] == board[x + 1, y - 1]) return -1;
                    // 3목이 아닌 경우 닫혔는지 확인
                    if (x == 1 || y == edgeCount - 2 || board[x - 2, y + 2] != 0) count--;
                    if (x == edgeCount - 1 || y == 0 || board[x + 1, y - 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make2mok3(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y])
                {
                    count = 2;
                    // 3목인 경우
                    if (x > 1 && board[x, y] == board[x - 2, y]) return -1;
                    if (x < edgeCount - 1 && board[x, y] == board[x + 1, y]) return -1;
                    // 3목이 아닌 경우 닫혔는지 확인
                    if (x == 1 || board[x - 2, y] != 0) count--;
                    if (x == edgeCount - 1 || board[x + 1, y] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        public int make2mok4(int x, int y)
        {
            int count = -1;
            try
            {
                if (board[x, y] == board[x - 1, y - 1])
                {
                    count = 2;
                    // 3목인 경우
                    if (x > 1 && y > 1 && board[x, y] == board[x - 2, y - 2]) return -1;
                    if (x < edgeCount - 1 && y < edgeCount - 1 && board[x, y] == board[x + 1, y + 1]) return -1;
                    // 3목이 아닌 경우 닫혔는지 확인
                    if (x == 1 || y == 1 || board[x - 2, y - 2] != 0) count--;
                    if (x == edgeCount - 1 || y == edgeCount - 1 || board[x + 1, y + 1] != 0) count--;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            return count;
        }

        /*------------- 현재 두려는 수의 가치를 판단하여 책정하는 부분 -------------*/

        private int evaluate(Horse horse)
        {
            // 각각의 돌이 놓여진 경우를 체크하는 변수
            int sum = 0;
            int open3 = 0;
            int close3 = 0;
            int half3 = 0;
            int open4 = 0;
            int close4 = 0;
            int half4 = 0;
            int open2 = 0;
            int close2 = 0;
            int half2 = 0;
            int count;

            // 모든 바둑판을 검사
            for (int i = 0; i < edgeCount; i++)
            {
                for (int j = 0; j < edgeCount; j++)
                {
                    // 자신의 돌이 있는 경우
                    if (board[i, j] == horse)
                    {
                        // 5목이 만들어진 경우
                        if (make5mok1(i, j) || make5mok2(i, j) || make5mok3(i, j) || make5mok4(i, j) || make5mok5(i, j) ||
                                make5mok6(i, j) || make5mok7(i, j) || make5mok8(i, j) || make5mok9(i, j) || make5mok10(i, j) ||
                                make5mok11(i, j) || make5mok12(i, j) || make5mok13(i, j) || make5mok14(i, j) || make5mok15(i, j) ||
                                make5mok16(i, j) || make5mok17(i, j) || make5mok18(i, j) || make5mok19(i, j) || make5mok20(i, j))
                        {
                            return 1000000;
                        }

                        /********** 단순히 전체에서 4목이 만들어진 경우만 체크 **********/

                        count = make4mok1(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok2(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok3(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok4(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok5(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok6(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok7(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok8(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok9(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok10(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok11(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok12(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok13(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok14(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok15(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        count = make4mok16(i, j);
                        if (count == 2) open4++;
                        else if (count == 1) half4++;
                        else if (count == 0) close4++;

                        /********** 단순히 전체에서 3목이 만들어진 경우만 체크 **********/

                        count = make3mok1(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok2(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok3(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok4(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok5(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok6(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok7(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok8(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok9(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok10(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok11(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        count = make3mok12(i, j);
                        if (count == 2) open3++;
                        else if (count == 1) half3++;
                        else if (count == 0) close3++;

                        /********** 단순히 전체에서 2목이 만들어진 경우만 체크 **********/

                        count = make2mok1(i, j);
                        if (count == 2) open2++;
                        else if (count == 1) half2++;
                        else if (count == 0) close2++;

                        count = make2mok2(i, j);
                        if (count == 2) open2++;
                        else if (count == 1) half2++;
                        else if (count == 0) close2++;

                        count = make2mok3(i, j);
                        if (count == 2) open2++;
                        else if (count == 1) half2++;
                        else if (count == 0) close2++;

                        count = make2mok4(i, j);
                        if (count == 2) open2++;
                        else if (count == 1) half2++;
                        else if (count == 0) close2++;

                        /********** 바둑돌이 중간에 가까울 수록 가중치를 계산 **********/

                        int middle = edgeCount / 2;
                        if (i > middle)
                        {
                            sum += 500 - ((i - middle) * 20);
                        }
                        else
                        {
                            sum += 500 - (middle - i) * 20;
                        }
                        if (j > middle)
                        {
                            sum += 500 - ((j - middle) * 20);
                        }
                        else
                        {
                            sum += 500 - (middle - j) * 20;
                        }
                    }
                }
            }

            // 가중치를 계산한 합을 sum에 더하여 반환
            sum += open4 * 200000;
            sum += half4 * 15000;
            sum += close4 * 1500;
            sum += open3 * 4000;
            sum += half3 * 1500;
            sum += close3 * 300;
            sum += open2 * 1500;
            sum += half2 * 300;
            sum += close2 * 50;
            return sum;
        }

        // AlphaBetaPruning 알고리즘 함수
        int AlphaBetaPruning(int level, int alpha, int beta)
        {
            // 만약에 현재 최대 깊이인 limit에 도달한 경우
            if (level == limit)
            {
                // 플레이어의 평가 가치에서 AI의 평가 가치를 뺀 수를 반환 (플레이어의 수 가치를 약간 더 높게 보기)
                return evaluate(aiPlayer) - evaluate(userPlayer);
            }
            // MAX 부분에 해당하는 경우
            if (level % 2 == 0)
            {
                // 더이상 작아질 수 없는 수를 max로 설정
                int max = -1000000;
                // 탐색을 끝내는 경우 find를 1로 설정
                int find = 0;
                // 전체 바둑판을 모두 탐색
                for (int i = 0; i < edgeCount; i++)
                {
                    for (int j = 0; j < edgeCount; j++)
                    {
                        // 현재 위치에 둘 수 있는 경우
                        if (board[i, j] == 0)
                        {
                            // 잠시 그 수를 둔 것으로 설정
                            board[i, j] = aiPlayer;
                            // 재귀적 호출
                            int e = AlphaBetaPruning(level + 1, alpha, beta);
                            // 다시 그 수를 두지 않은 것으로 설정
                            board[i, j] = 0;
                            // 만약에 더욱 효율적인 수를 찾은 경우
                            if (max < e)
                            {
                                // 그 수로 타겟을 설정
                                max = e;
                                if (level == 0)
                                {
                                    targetX = i;
                                    targetY = j;
                                }
                            }
                            // alpha값 갱신
                            if (alpha < max)
                            {
                                alpha = max;
                                // 만약에 현재 알파값이 베타값보다 크다면 더이상 노드를 볼 필요가 없음
                                if (alpha >= beta) find = 1;
                            }
                        }
                        if (find == 1) break;
                    }
                    if (find == 1) break;
                }
                return max;
            }
            // MIN 부분에 해당하는 경우
            else
            {
                // 더이상 커질 수 없는 수를 min으로 설정
                int min = 1000000;
                // 탐색을 끝내는 경우 find를 1로 설정
                int find = 0;
                // 전체 바둑판을 모두 탐색
                for (int i = 0; i < edgeCount; i++)
                {
                    for (int j = 0; j < edgeCount; j++)
                    {
                        // 현재 위치에 둘 수 있는 경우
                        if (board[i, j] == 0)
                        {
                            // 잠시 그 수를 둔 것으로 설정
                            board[i, j] = userPlayer;
                            // 재귀적 호출
                            int e = AlphaBetaPruning(level + 1, alpha, beta);
                            // 다시 그 수를 두지 않은 것으로 설정
                            board[i, j] = 0;
                            // 더욱 작은 수를 찾은 경우
                            if (min > e) min = e;
                            // beta값 갱신
                            if (beta > min)
                            {
                                beta = min;
                                // 만약에 현재 알파값이 베타값보다 크다면 더이상 노드를 볼 필요가 없음
                                if (alpha >= beta) find = 1;
                            }
                        }
                        if (find == 1) break;
                    }
                    if (find == 1) break;
                }
                return min;
            }
        }
    }
}