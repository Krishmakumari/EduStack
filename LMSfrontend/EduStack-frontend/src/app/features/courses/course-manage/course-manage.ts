import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  CourseService,
  CourseDetailResponse,
  SectionResponse,
  LessonResponse,
  AddSectionRequest,
  AddLessonRequest,
} from '../../../services/course.service';

@Component({
  selector: 'app-course-manage',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './course-manage.html',
  styleUrl: './course-manage.css',
})
export class CourseManage implements OnInit {
  courseId = '';
  course: CourseDetailResponse | null = null;
  loading = true;
  errorMessage = '';
  actionMessage = '';

  // Section form
  showSectionForm = false;
  editSectionId: string | null = null;
  sectionTitle = '';
  sectionOrder = 1;

  // Lesson form
  showLessonFormFor: string | null = null; // sectionId
  editLessonId: string | null = null;
  lessonTitle = '';
  lessonVideoUrl = '';
  lessonContent = '';
  lessonDuration = 0;
  lessonOrder = 1;
  lessonFreePreview = false;

  constructor(
    private courseService: CourseService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    this.courseId = this.route.snapshot.paramMap.get('id')!;
    this.loadCourse();
  }

  loadCourse() {
    this.courseService.getCourseById(this.courseId).subscribe({
      next: (data) => {
        this.course = data;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load course.';
        this.loading = false;
      },
    });
  }

  // ─── Section CRUD ─────────────────────────────────────────────────────────

  openAddSection() {
    this.editSectionId = null;
    this.sectionTitle = '';
    this.sectionOrder = (this.course?.sections.length ?? 0) + 1;
    this.showSectionForm = true;
  }

  openEditSection(section: SectionResponse) {
    this.editSectionId = section.sectionId;
    this.sectionTitle = section.title;
    this.sectionOrder = section.order;
    this.showSectionForm = true;
  }

  cancelSectionForm() {
    this.showSectionForm = false;
    this.editSectionId = null;
  }

  saveSection() {
    const data: AddSectionRequest = { title: this.sectionTitle, order: this.sectionOrder };

    if (this.editSectionId) {
      this.courseService.updateSection(this.editSectionId, data).subscribe({
        next: (updated) => {
          const idx = this.course!.sections.findIndex(s => s.sectionId === this.editSectionId);
          if (idx >= 0) {
            this.course!.sections[idx].title = updated.title;
            this.course!.sections[idx].order = updated.order;
          }
          this.cancelSectionForm();
          this.showAction('Section updated.');
        },
        error: (err) => this.showAction(err.error?.message || 'Update failed.'),
      });
    } else {
      this.courseService.addSection(this.courseId, data).subscribe({
        next: (section) => {
          this.course!.sections.push(section);
          this.cancelSectionForm();
          this.showAction('Section added.');
        },
        error: (err) => this.showAction(err.error?.message || 'Add failed.'),
      });
    }
  }

  deleteSection(sectionId: string) {
    if (!confirm('Delete this section and all its lessons?')) return;
    this.courseService.deleteSection(sectionId).subscribe({
      next: () => {
        this.course!.sections = this.course!.sections.filter(s => s.sectionId !== sectionId);
        this.showAction('Section deleted.');
      },
      error: (err) => this.showAction(err.error?.message || 'Delete failed.'),
    });
  }

  // ─── Lesson CRUD ──────────────────────────────────────────────────────────

  openAddLesson(sectionId: string, lessonCount: number) {
    this.editLessonId = null;
    this.showLessonFormFor = sectionId;
    this.lessonTitle = '';
    this.lessonVideoUrl = '';
    this.lessonContent = '';
    this.lessonDuration = 0;
    this.lessonOrder = lessonCount + 1;
    this.lessonFreePreview = false;
  }

  openEditLesson(sectionId: string, lesson: LessonResponse) {
    this.editLessonId = lesson.lessonId;
    this.showLessonFormFor = sectionId;
    this.lessonTitle = lesson.title;
    this.lessonVideoUrl = lesson.videoUrl || '';
    this.lessonContent = lesson.content || '';
    this.lessonDuration = lesson.durationInSeconds;
    this.lessonOrder = lesson.order;
    this.lessonFreePreview = lesson.isFreePreview;
  }

  cancelLessonForm() {
    this.showLessonFormFor = null;
    this.editLessonId = null;
  }

  saveLesson() {
    const sectionId = this.showLessonFormFor!;
    const data: AddLessonRequest = {
      title: this.lessonTitle,
      videoUrl: this.lessonVideoUrl || null,
      content: this.lessonContent || null,
      durationInSeconds: this.lessonDuration,
      order: this.lessonOrder,
      isFreePreview: this.lessonFreePreview,
    };

    if (this.editLessonId) {
      this.courseService.updateLesson(this.editLessonId, data).subscribe({
        next: (updated) => {
          const section = this.course!.sections.find(s => s.sectionId === sectionId);
          if (section) {
            const idx = section.lessons.findIndex(l => l.lessonId === this.editLessonId);
            if (idx >= 0) section.lessons[idx] = updated;
          }
          this.cancelLessonForm();
          this.showAction('Lesson updated.');
        },
        error: (err) => this.showAction(err.error?.message || 'Update failed.'),
      });
    } else {
      this.courseService.addLesson(sectionId, data).subscribe({
        next: (lesson) => {
          const section = this.course!.sections.find(s => s.sectionId === sectionId);
          if (section) section.lessons.push(lesson);
          this.cancelLessonForm();
          this.showAction('Lesson added.');
        },
        error: (err) => this.showAction(err.error?.message || 'Add failed.'),
      });
    }
  }

  deleteLesson(sectionId: string, lessonId: string) {
    if (!confirm('Delete this lesson?')) return;
    this.courseService.deleteLesson(lessonId).subscribe({
      next: () => {
        const section = this.course!.sections.find(s => s.sectionId === sectionId);
        if (section) section.lessons = section.lessons.filter(l => l.lessonId !== lessonId);
        this.showAction('Lesson deleted.');
      },
      error: (err) => this.showAction(err.error?.message || 'Delete failed.'),
    });
  }

  formatDuration(sec: number): string {
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return m > 0 ? `${m}m ${s}s` : `${s}s`;
  }

  private showAction(msg: string) {
    this.actionMessage = msg;
    setTimeout(() => (this.actionMessage = ''), 3000);
  }
}
