using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core;

public abstract class EntityHandler : INotifyPropertyChanged
{
    public EntityHandler(IWebMusicEntity entity, CoreService service) {
        Entity = entity;
        Service = service;
    }

    protected CoreService Service { get; private set; }

    public IWebMusicEntity Entity { get; private set; }

    public ImageSource Cover { get; set; }



    public string Query { get; set; }

    public virtual bool SupportCover => CoverUri != null;

    public virtual string CoverUri { get; } = null;

    public virtual EntityType EntityType { get; } = EntityType.Unknown;


    public virtual List<Caption> Titles { get; } = null;

    public virtual string Title => Titles?.FirstOrDefault()?.Title;

    public virtual List<Caption> SecondTitles { get; } = null;

    public virtual string SecondTitle => SecondTitles?.FirstOrDefault()?.Title;

    public virtual List<Caption> ThirdTitles { get; } = null;

    public virtual string ThirdTitle => ThirdTitles?.FirstOrDefault()?.Title;


    public virtual string Description { get; } = null;

    public virtual TimeSpan? Duration { get; } = null;

    public virtual int? OrderNumber { get; } = null;

    public virtual bool IsBest { get; } = false;


    public virtual bool SupportOpen => Query != null;

    public virtual bool SupportDownload { get; } = false;

    public virtual bool SupportPlay { get; } = false;


    public virtual bool SupportLike { get; } = false;

    public virtual bool Liked { get; } = false;

    public virtual bool SupportDislike { get; } = false;

    public virtual bool Disliked { get; } = false;


    public virtual bool IsCaption { get; } = false;


    public virtual string GetUrl() {
        return null;
    }

    public virtual string GetUrl(EntityHandlerUrlType urlType) {
        return null;
    }

    public virtual bool SupportUrlType(EntityHandlerUrlType urlType) {
        return false;
    }


    public virtual Task SetLikeStateAsync(LikeState state, CancellationToken cancellationToken) {
        return Task.FromException(
            new NotSupportedException("Сущность не поддерживает установку/снятие лайков."));
    }

    public virtual Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        return Task.FromException<List<IWebMusicEntity>>(
            new NotSupportedException("Сущность не генерирует ленту."));
    }

    public virtual Task<List<StartDownloadInfo>> GetStartDownloadInfoAsync(CancellationToken cancellationToken) {
        return Task.FromException<List<StartDownloadInfo>>(
            new NotSupportedException("Сущность не поддерживает формирование списка для загрузки."));
    }

    public virtual void UpdateLikeStateFromLibrary() {
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string prop = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public override string ToString() {
        return $"{GetType().Name}: {Title} - {SecondTitle}";
    }
}
