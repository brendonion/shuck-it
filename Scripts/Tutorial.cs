using Godot;
using System.Collections.Generic;

public class Tutorial : Node2D {

    public int currentPageIndex;

    public AnimationPlayer animPlayer;

    public Button prevButtton;
    public Button nextButtton;

    public List<Node2D> pages = new List<Node2D>();

    public override void _Ready() {
        this.animPlayer  = (AnimationPlayer) FindNode("AnimationPlayer");
        this.prevButtton = (Button) FindNode("PrevButton");
        this.nextButtton = (Button) FindNode("NextButton");

        this.pages.Add((Node2D) FindNode("Page1"));
        this.pages.Add((Node2D) FindNode("Page2"));
        this.pages.Add((Node2D) FindNode("Page3"));
        this.pages.Add((Node2D) FindNode("Page4"));
        this.pages.Add((Node2D) FindNode("Page5"));
        this.pages.Add((Node2D) FindNode("Page6"));
        this.pages.Add((Node2D) FindNode("Page7"));

        this.currentPageIndex = 0;
        this.pages[this.currentPageIndex].Visible = true;
        this.animPlayer.Play("Page1");
        this.CheckButtonVisibility();
    }

    public void _OnNextPressed() {
        this.currentPageIndex++;
        this.pages[this.currentPageIndex - 1].Visible = false;
        this.pages[this.currentPageIndex].Visible     = true;
        this.animPlayer.Play("Page" + (this.currentPageIndex + 1));
        this.CheckButtonVisibility();
    }

    public void _OnPreviousPressed() {
        this.currentPageIndex--;
        this.pages[this.currentPageIndex + 1].Visible = false;
        this.pages[this.currentPageIndex].Visible     = true;
        this.animPlayer.Play("Page" + (this.currentPageIndex + 1));
        this.CheckButtonVisibility();
    }

    public void _OnExitPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void CheckButtonVisibility() {
        this.prevButtton.Visible = this.currentPageIndex != 0;
        this.nextButtton.Visible = this.currentPageIndex != this.pages.Count - 1;
    }
}
