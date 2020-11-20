using Godot;
using System.Collections.Generic;

public class Tutorial : Node2D {

    public int currentPageIndex;

    public AnimationPlayer animPlayer;

    public List<Node2D> pages = new List<Node2D>();

    public override void _Ready() {
        this.animPlayer = (AnimationPlayer) FindNode("AnimationPlayer");

        this.pages.Add((Node2D) FindNode("Page1"));
        this.pages.Add((Node2D) FindNode("Page2"));
        this.pages.Add((Node2D) FindNode("Page3"));
        this.pages.Add((Node2D) FindNode("Page4"));
        this.pages.Add((Node2D) FindNode("Page5"));
        this.pages.Add((Node2D) FindNode("Page6"));
        this.pages.Add((Node2D) FindNode("Page7"));

        this.currentPageIndex = 0;
        this.pages[this.currentPageIndex].Visible = true;
    }

    public void _OnNextPressed() {
        this.currentPageIndex++;
        this.pages[this.currentPageIndex - 1].Visible = false;
        this.pages[this.currentPageIndex].Visible     = true;
    }

    public void _OnPreviousPressed() {
        this.currentPageIndex--;
        this.pages[this.currentPageIndex + 1].Visible = false;
        this.pages[this.currentPageIndex].Visible     = true;
    }

    public void _OnExitPressed() {

    }
}
